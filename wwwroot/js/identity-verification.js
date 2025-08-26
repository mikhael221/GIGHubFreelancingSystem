// Identity Verification JavaScript
class IdentityVerification {
    constructor() {
        this.video = null;
        this.canvas = null;
        this.stream = null;
        this.isCameraActive = false;
    }

    initializeElements() {
        this.video = document.getElementById('video');
        this.canvas = document.getElementById('canvas');
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
        try {
            this.showLoading(true);
            
            // Request camera access
            this.stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    width: { ideal: 640 },
                    height: { ideal: 480 },
                    facingMode: 'user'
                }
            });

            // Set video source
            this.video.srcObject = this.stream;
            await this.video.play();

            // Update UI
            this.isCameraActive = true;
            this.startCameraBtn.style.display = 'none';
            this.capturePhotoBtn.style.display = 'inline-block';
            
            this.showNotification('Camera started successfully', 'success');
            this.showLoading(false);

        } catch (error) {
            console.error('Error starting camera:', error);
            this.showError('Failed to start camera. Please ensure camera permissions are granted.');
            this.showLoading(false);
        }
    }

    stopCamera() {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }
        this.isCameraActive = false;
        this.video.srcObject = null;
    }

    captureFace() {
        if (!this.isCameraActive) {
            this.showError('Camera is not active. Please start the camera first.');
            return;
        }

        try {
            // Set canvas dimensions to match video
            this.canvas.width = this.video.videoWidth;
            this.canvas.height = this.video.videoHeight;

            // Draw video frame to canvas
            const context = this.canvas.getContext('2d');
            context.drawImage(this.video, 0, 0, this.canvas.width, this.canvas.height);

            // Convert to base64
            const imageData = this.canvas.toDataURL('image/jpeg', 0.8);
            
            // Store the image data
            this.liveFaceImageData.value = imageData;
            
            // Display captured image
            this.showCapturedImage(imageData);
            
            // Stop camera
            this.stopCamera();
            
            // Update UI
            this.capturePhotoBtn.style.display = 'none';
            this.retakePhotoBtn.style.display = 'inline-block';
            
            this.showNotification('Photo captured successfully!', 'success');

        } catch (error) {
            console.error('Error capturing photo:', error);
            this.showError('Failed to capture photo. Please try again.');
        }
    }

    showCapturedImage(imageData) {
        this.capturedImage.src = imageData;
        this.capturedImageContainer.style.display = 'block';
    }

    retakePhoto() {
        // Clear previous image
        this.liveFaceImageData.value = '';
        this.capturedImageContainer.style.display = 'none';
        
        // Reset UI
        this.retakePhotoBtn.style.display = 'none';
        this.startCameraBtn.style.display = 'inline-block';
        
        // Start camera again
        this.startCamera();
    }

    async submitVerification() {
        if (!this.liveFaceImageData.value) {
            this.showError('Please capture a live photo before submitting.');
            return false;
        }

        this.showLoading(true);
        return true;
    }

    dataURLToBlob(dataURL) {
        const arr = dataURL.split(',');
        const mime = arr[0].match(/:(.*?);/)[1];
        const bstr = atob(arr[1]);
        let n = bstr.length;
        const u8arr = new Uint8Array(n);
        
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        
        return new Blob([u8arr], { type: mime });
    }

    showError(message) {
        this.showNotification(message, 'error');
    }

    showSuccess(message) {
        this.showNotification(message, 'success');
    }

    showNotification(message, type) {
        const alertClass = type === 'error' ? 'alert-danger' : 'alert-success';
        const icon = type === 'error' ? 'exclamation-circle' : 'check-circle';
        
        const notification = document.createElement('div');
        notification.className = `alert ${alertClass} alert-dismissible fade show`;
        notification.innerHTML = `
            <i class="fas fa-${icon} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        // Insert at the top of the card body
        const cardBody = document.querySelector('.card-body');
        if (cardBody) {
            cardBody.insertBefore(notification, cardBody.firstChild);
            
            // Auto-remove after 5 seconds
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 5000);
        }
    }

    showLoading(show) {
        const submitBtn = document.getElementById('submitBtn');
        if (submitBtn) {
            if (show) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';
            } else {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="fas fa-paper-plane me-2"></i>Submit Verification';
            }
        }
    }

    destroy() {
        this.stopCamera();
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on the verification page
    if (document.getElementById('video')) {
        const identityVerification = new IdentityVerification();
        identityVerification.initializeElements();
        identityVerification.bindEvents();
    }
});
