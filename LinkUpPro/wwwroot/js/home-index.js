function handleImageSelect(event) {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    document.getElementById('imagePreview').src = e.target.result;
                    document.getElementById('imagePreview').classList.remove('hidden');
                    document.getElementById('videoPreviewContainer').classList.add('hidden');
                    document.getElementById('mediaPreviewContainer').classList.remove('hidden');
                    document.getElementById('MediaType').value = '0'; // Image
                    document.getElementById('VideoUrl').value = '';
                    document.getElementById('videoUrlContainer').classList.add('hidden');
                }
                reader.readAsDataURL(file);
            }
        }

        function toggleVideoInput() {
            const container = document.getElementById('videoUrlContainer');
            container.classList.toggle('hidden');
            if (!container.classList.contains('hidden')) {
                document.getElementById('VideoUrl').focus();
            }
        }

        function handleVideoInput(event) {
            const url = event.target.value;
            const videoId = extractYouTubeId(url);
            
            if (videoId) {
                document.getElementById('videoPreview').src = `https://www.youtube.com/embed/${videoId}`;
                document.getElementById('videoPreviewContainer').classList.remove('hidden');
                document.getElementById('imagePreview').classList.add('hidden');
                document.getElementById('mediaPreviewContainer').classList.remove('hidden');
                document.getElementById('MediaType').value = '1'; // Video
                document.getElementById('ImageFile').value = '';
            } else if (url === '') {
                clearMedia();
            }
        }

        function extractYouTubeId(url) {
            const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=|shorts\/)([^#\&\?]*).*/;
            const match = url.match(regExp);
            return (match && match[2].length === 11) ? match[2] : null;
        }

        function clearMedia() {
            document.getElementById('ImageFile').value = '';
            document.getElementById('VideoUrl').value = '';
            document.getElementById('MediaType').value = '0';
            
            document.getElementById('mediaPreviewContainer').classList.add('hidden');
            document.getElementById('imagePreview').classList.add('hidden');
            document.getElementById('videoPreviewContainer').classList.add('hidden');
            document.getElementById('videoUrlContainer').classList.add('hidden');
            document.getElementById('imagePreview').src = '#';
            document.getElementById('videoPreview').src = '';
        }

        async function handleReaction(event, form) {
            event.preventDefault();
            
            const btn = form.querySelector('button');
            const icon = btn.querySelector('svg') || btn.querySelector('i');
            const isLike = form.querySelector('input[name="isLike"]').value === 'true';
            
            const postId = form.querySelector('input[name="postId"]').value;

            // Find the other form's button to un-highlight if switching
            const parentDiv = form.parentElement;
            const otherFormIndex = isLike ? 1 : 0;
            const otherForm = parentDiv.querySelectorAll('form')[otherFormIndex];
            const otherBtn = otherForm.querySelector('button');
            const otherIcon = otherBtn.querySelector('svg') || otherBtn.querySelector('i');

            const likesCountEl = document.getElementById(`likes-count-${postId}`);
            const dislikesCountEl = document.getElementById(`dislikes-count-${postId}`);
            let likesCount = parseInt(likesCountEl.innerText);
            let dislikesCount = parseInt(dislikesCountEl.innerText);

            // Optimistic UI update
            const wasActive = isLike ? btn.classList.contains('text-primary') : btn.classList.contains('text-destructive');
            
            if (wasActive) {
                // Remove active state
                btn.classList.remove(isLike ? 'text-primary' : 'text-destructive');
                icon.classList.remove('fill-current');
                if (isLike) likesCount--; else dislikesCount--;
            } else {
                // Set active state
                btn.classList.add(isLike ? 'text-primary' : 'text-destructive');
                icon.classList.add('fill-current');
                if (isLike) likesCount++; else dislikesCount++;
                
                // Remove active from other
                const otherWasActive = otherBtn.classList.contains(isLike ? 'text-destructive' : 'text-primary');
                if (otherWasActive) {
                    otherBtn.classList.remove(isLike ? 'text-destructive' : 'text-primary');
                    otherIcon.classList.remove('fill-current');
                    if (isLike) dislikesCount--; else likesCount--;
                }
            }
            
            likesCountEl.innerText = likesCount;
            dislikesCountEl.innerText = dislikesCount;

            const formData = new FormData(form);
            try {
                await fetch(form.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
            } catch (e) {
                console.error('Error toggling reaction', e);
            }
        }

        function disableSubmit(form) {
            const btn = form.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                btn.innerText = 'Enviando...';
                btn.classList.add('opacity-50', 'pointer-events-none');
            }
        }

        async function handleCommentSubmit(event, form) {
            event.preventDefault();
            const btn = form.querySelector('button[type="submit"]');
            const originalText = btn ? btn.innerText : '';
            
            if (btn) {
                btn.disabled = true;
                const isDelete = form.action.includes('Delete');
                btn.innerText = isDelete ? 'Eliminando...' : 'Enviando...';
                btn.classList.add('opacity-50', 'pointer-events-none');
            }

            const formData = new FormData(form);
            try {
                const response = await fetch(form.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
                
                const data = await response.json();
                if(data.success) {
                    // Re-fetch the current page silently to update the comments DOM
                    const pageRes = await fetch(window.location.href);
                    const pageText = await pageRes.text();
                    const parser = new DOMParser();
                    const doc = parser.parseFromString(pageText, 'text/html');
                    
                    const postIdInput = form.querySelector('input[name="PostId"]') || form.querySelector('input[name="postId"]');
                    const postId = postIdInput.value;
                    
                    const newCommentsSection = doc.getElementById('comments-' + postId);
                    const currentCommentsSection = document.getElementById('comments-' + postId);
                    
                    if(newCommentsSection && currentCommentsSection) {
                        currentCommentsSection.innerHTML = newCommentsSection.innerHTML;
                    }
                    
                    // Also update the comment count text
                    const newCommentCount = doc.getElementById('comment-count-text-' + postId);
                    const currentCommentCount = document.getElementById('comment-count-text-' + postId);
                    if(newCommentCount && currentCommentCount) {
                        currentCommentCount.innerText = newCommentCount.innerText;
                    }
                } else {
                    alert(data.message || 'Error al procesar la solicitud');
                    if (btn) {
                        btn.disabled = false;
                        btn.innerText = originalText;
                        btn.classList.remove('opacity-50', 'pointer-events-none');
                    }
                }
            } catch (e) {
                console.error(e);
                if (btn) {
                    btn.disabled = false;
                    btn.innerText = originalText;
                    btn.classList.remove('opacity-50', 'pointer-events-none');
                }
            }
        }