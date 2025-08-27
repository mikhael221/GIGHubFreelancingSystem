// Clean Identity Verification JavaScript
class IdentityVerification {
    constructor() {
        this.video = null;
        this.canvas = null;
        this.stream = null;
        this.isCameraActive = false;
        this.isStarting = false;
        this.tracks = [];
    }

    initializeElements() {
        this.video = document.getElementById('video');
        this.canvas = document.getElementById('canvas');
        this.cameraContainer = document.getElementById('cameraContainer');
        this.startCameraBtn = document.getElementById('startCamera');
        this.capturePhotoBtn = document.getElementById('capturePhoto');
        this.retakePhotoBtn = document.getElementById('retakePhoto');
        this.capturedImageContainer = document.getElementById('capturedImageContainer');
        this.capturedImage = document.getElementById('capturedImage');
        this.liveFaceImageData = document.getElementById('liveFaceImageData');
    }

    bindEvents() {
        if (this.startCameraBtn) {
            this.startCameraBtn.addEventListener('click', () => this.startCamera());
        }
        if (this.capturePhotoBtn) {
            this.capturePhotoBtn.addEventListener('click', () => this.captureFace());
        }
        if (this.retakePhotoBtn) {
            this.retakePhotoBtn.addEventListener('click', () => this.retakePhoto());
        }
    }

    async startCamera() {
        if (this.isStarting) return;
        this.isStarting = true;

        try {
            this.stopCamera();
            await new Promise(resolve => setTimeout(resolve, 300));

            this.stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    width: { ideal: 640 },
                    height: { ideal: 480 },
                    facingMode: 'user'
                }
            });

            this.tracks = this.stream.getTracks();
            this.video.srcObject = this.stream;

            await this.playVideoWithRetry();
            await this.waitForVideoReady();

            this.isCameraActive = true;
            this.updateUIForCameraActive();

        } catch (error) {
            this.handleCameraError(error);
            this.resetCameraUI();
        } finally {
            this.isStarting = false;
        }
    }

    async playVideoWithRetry() {
        let attempts = 0;
        const maxAttempts = 3;

        while (attempts < maxAttempts) {
            try {
                await this.video.play();
                return;
            } catch (error) {
                attempts++;
                if (error.name === 'AbortError' && attempts < maxAttempts) {
                    await new Promise(resolve => setTimeout(resolve, 300));
                    continue;
                } else if (attempts >= maxAttempts) {
                    throw error;
                }
            }
        }
    }

    async waitForVideoReady() {
        return new Promise((resolve, reject) => {
            let attempts = 0;
            const maxAttempts = 30;

            const checkReady = () => {
                attempts++;
                if (this.video.videoWidth > 0 && this.video.videoHeight > 0) {
                    resolve();
                } else if (attempts >= maxAttempts) {
                    reject(new Error('Video dimensions timeout'));
                } else {
                    setTimeout(checkReady, 100);
                }
            };

            checkReady();
        });
    }

    stopCamera() {
        if (this.tracks && this.tracks.length > 0) {
            this.tracks.forEach(track => {
                if (track && track.readyState === 'live') {
                    track.stop();
                }
            });
            this.tracks = [];
        }

        if (this.stream) {
            const streamTracks = this.stream.getTracks();
            streamTracks.forEach(track => {
                if (track.readyState === 'live') {
                    track.stop();
                }
            });
            this.stream = null;
        }

        if (this.video) {
            this.video.pause();
            this.video.currentTime = 0;

            if (this.video.srcObject) {
                const videoStream = this.video.srcObject;
                if (videoStream && videoStream.getTracks) {
                    videoStream.getTracks().forEach(track => {
                        track.stop();
                    });
                }
                this.video.srcObject = null;
            }

            this.video.src = '';
            this.video.removeAttribute('src');
            this.video.load();
        }

        document.querySelectorAll('video').forEach(video => {
            if (video.srcObject) {
                const stream = video.srcObject;
                if (stream && stream.getTracks) {
                    stream.getTracks().forEach(track => {
                        if (track.readyState === 'live') {
                            track.stop();
                        }
                    });
                }
                video.srcObject = null;
            }
            video.pause();
            video.currentTime = 0;
            video.src = '';
            video.removeAttribute('src');
            video.load();
        });

        this.isCameraActive = false;
    }

    forceStopCamera() {
        this.stopCamera();
    }

    captureFace() {
        if (!this.isCameraActive) {
            this.showError('Camera is not active. Please start the camera first.');
            return;
        }

        if (!this.stream || !this.stream.active) {
            this.showError('Camera stream is not active. Please restart the camera.');
            return;
        }

        if (!this.video.videoWidth || !this.video.videoHeight) {
            setTimeout(() => {
                this.attemptCapture();
            }, 500);
            return;
        }

        this.attemptCapture();
    }

    attemptCapture() {
        try {
            if (this.capturePhotoBtn) {
                this.capturePhotoBtn.disabled = true;
                this.capturePhotoBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Capturing...';
            }

            this.canvas.width = this.video.videoWidth || 640;
            this.canvas.height = this.video.videoHeight || 480;

            const context = this.canvas.getContext('2d');
            context.drawImage(this.video, 0, 0, this.canvas.width, this.canvas.height);

            const imageData = this.canvas.toDataURL('image/jpeg', 0.8);

            if (imageData.length < 1000) {
                throw new Error('Captured image data is too small');
            }

            if (this.liveFaceImageData) {
                this.liveFaceImageData.value = imageData;
            }

            this.showCapturedImage(imageData);
            this.updateUIForPhotoCapture();

            setTimeout(() => {
                this.stopCamera();
            }, 50);

        } catch (error) {
            this.showError('Failed to capture photo. Please try again.');

            if (this.capturePhotoBtn) {
                this.capturePhotoBtn.disabled = false;
                this.capturePhotoBtn.innerHTML = '<svg viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg" stroke-width="3" stroke="#000000" fill="none" class="w-20 h-20"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"><circle cx="32" cy="32" r="25"></circle><line x1="38.33" y1="21.87" x2="52.52" y2="46.27"></line><line x1="12.09" y1="16.88" x2="26.89" y2="42.33"></line><line x1="44.32" y1="32.16" x2="29.72" y2="56.9"></line><line x1="35.43" y1="7.23" x2="20.91" y2="32.05"></line><line x1="38.22" y1="42.5" x2="9.23" y2="42.33"></line><line x1="54.9" y1="21.95" x2="26.92" y2="21.78"></line></g></svg>';
            }
        }
    }

    showCapturedImage(imageData) {
        const isInModal = !document.getElementById('cameraModal').classList.contains('hidden');

        if (isInModal) {
            this.showModalCapturedImage(imageData);
        } else {
            this.showMainPageCapturedImage(imageData);
        }
    }

    showModalCapturedImage(imageData) {
        const modalImage = document.getElementById('modalCapturedImage');
        const videoElement = document.getElementById('video');
        const cameraOverlay = document.getElementById('cameraOverlay');
        const successControls = document.getElementById('captureSuccessControls');

        if (modalImage) {
            modalImage.src = imageData;
            modalImage.style.display = 'block';
        }
        if (videoElement) {
            videoElement.style.display = 'none';
        }
        if (cameraOverlay) {
            cameraOverlay.style.display = 'none';
        }
        if (successControls) {
            successControls.style.display = 'block';
        }
    }

    showMainPageCapturedImage(imageData) {
        if (this.capturedImage && this.capturedImageContainer) {
            this.capturedImage.src = imageData;
            this.capturedImageContainer.style.display = 'block';
        }
    }

    retakePhoto() {
        if (this.liveFaceImageData) {
            this.liveFaceImageData.value = '';
        }
        this.stopCamera();
        this.resetUI();
    }

    updateUIForCameraActive() {
        if (this.startCameraBtn) this.startCameraBtn.style.display = 'none';
        if (this.capturePhotoBtn) {
            this.capturePhotoBtn.style.display = 'inline-block';
            this.capturePhotoBtn.disabled = false;
            this.capturePhotoBtn.innerHTML = '<svg viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg" stroke-width="3" stroke="#000000" fill="none" class="w-20 h-20"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"><circle cx="32" cy="32" r="25"></circle><line x1="38.33" y1="21.87" x2="52.52" y2="46.27"></line><line x1="12.09" y1="16.88" x2="26.89" y2="42.33"></line><line x1="44.32" y1="32.16" x2="29.72" y2="56.9"></line><line x1="35.43" y1="7.23" x2="20.91" y2="32.05"></line><line x1="38.22" y1="42.5" x2="9.23" y2="42.33"></line><line x1="54.9" y1="21.95" x2="26.92" y2="21.78"></line></g></svg>';
        }
        if (this.cameraContainer) this.cameraContainer.style.display = 'block';

        const placeholder = document.getElementById('cameraPlaceholder');
        if (placeholder) placeholder.style.display = 'none';

        const overlay = document.getElementById('cameraOverlay');
        if (overlay) overlay.style.display = 'block';
    }

    updateUIForPhotoCapture() {
        if (this.capturePhotoBtn) this.capturePhotoBtn.style.display = 'none';
        if (this.retakePhotoBtn) this.retakePhotoBtn.style.display = 'inline-block';
    }

    resetUI() {
        const isInModal = !document.getElementById('cameraModal').classList.contains('hidden');

        if (isInModal) {
            this.resetModalUI();
        } else {
            this.resetMainPageUI();
        }

        if (this.startCameraBtn) this.startCameraBtn.style.display = 'inline-block';
        if (this.capturePhotoBtn) this.capturePhotoBtn.style.display = 'none';
        if (this.retakePhotoBtn) this.retakePhotoBtn.style.display = 'none';
    }

    resetModalUI() {
        const modalImage = document.getElementById('modalCapturedImage');
        const successControls = document.getElementById('captureSuccessControls');
        const placeholder = document.getElementById('cameraPlaceholder');
        const container = document.getElementById('cameraContainer');
        const video = document.getElementById('video');
        const overlay = document.getElementById('cameraOverlay');

        if (modalImage) {
            modalImage.style.display = 'none';
            modalImage.src = '';
        }
        if (successControls) successControls.style.display = 'none';
        if (placeholder) placeholder.style.display = 'block';
        if (container) container.style.display = 'none';
        if (video) video.style.display = 'block';
        if (overlay) overlay.style.display = 'block';
    }

    resetMainPageUI() {
        if (this.capturedImageContainer) {
            this.capturedImageContainer.style.display = 'none';
        }
    }

    resetCameraUI() {
        if (this.startCameraBtn) this.startCameraBtn.style.display = 'inline-block';
        if (this.capturePhotoBtn) this.capturePhotoBtn.style.display = 'none';
        if (this.cameraContainer) this.cameraContainer.style.display = 'none';

        const placeholder = document.getElementById('cameraPlaceholder');
        if (placeholder) placeholder.style.display = 'block';

        this.isCameraActive = false;
    }

    handleCameraError(error) {
        let errorMessage = '';

        switch (error.name) {
            case 'NotAllowedError':
                errorMessage = 'Camera access denied. Please allow camera permissions and try again.';
                break;
            case 'NotFoundError':
                errorMessage = 'No camera found on your device.';
                break;
            case 'NotReadableError':
                errorMessage = 'Camera is in use by another application.';
                break;
            case 'AbortError':
                return;
            default:
                errorMessage = 'Camera access failed. Please try again.';
        }

        this.showError(errorMessage);
    }

    showError(message) {
        alert(message);
    }

    destroy() {
        this.stopCamera();
    }
}

// Global camera cleanup
window.stopAllCameras = function () {
    if (window.identityVerification) {
        window.identityVerification.stopCamera();
    }

    document.querySelectorAll('video').forEach(video => {
        if (video.srcObject) {
            const stream = video.srcObject;
            if (stream && stream.getTracks) {
                const tracks = stream.getTracks();
                tracks.forEach(track => {
                    if (track.readyState === 'live') {
                        track.stop();
                    }
                });
            }
            video.srcObject = null;
        }

        video.pause();
        video.currentTime = 0;
        video.src = '';
        video.removeAttribute('src');
        video.load();
    });
};

// Initialize when DOM loads
document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('video')) {
        const identityVerification = new IdentityVerification();
        identityVerification.initializeElements();
        identityVerification.bindEvents();
        window.identityVerification = identityVerification;
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    window.stopAllCameras();
});

window.addEventListener('pagehide', () => {
    window.stopAllCameras();
});

document.addEventListener('visibilitychange', function () {
    if (document.hidden) {
        window.stopAllCameras();
    }
});

window.addEventListener('blur', () => {
    window.stopAllCameras();
});

// Modal observer to catch modal hide
if (typeof MutationObserver !== 'undefined') {
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                const modal = document.getElementById('cameraModal');
                if (modal && modal.classList.contains('hidden')) {
                    window.stopAllCameras();
                }
            }
        });
    });

    document.addEventListener('DOMContentLoaded', function () {
        const modal = document.getElementById('cameraModal');
        if (modal) {
            observer.observe(modal, { attributes: true });
        }
    });
}