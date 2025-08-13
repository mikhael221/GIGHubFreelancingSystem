document.addEventListener('DOMContentLoaded', function() {
    // Image upload functionality
    const fileInput = document.getElementById('dropzone-file');
    const uploadPlaceholder = document.getElementById('upload-placeholder');
    const uploadProgress = document.getElementById('upload-progress');
    const fileList = document.getElementById('file-list');
    const uploadStatus = document.getElementById('upload-status');
    const uploadPercentage = document.getElementById('upload-percentage');
    const progressBar = document.getElementById('progress-bar');
    const uploadedImagesContainer = document.getElementById('uploaded-images-container');

    if (!fileInput || !uploadPlaceholder || !uploadProgress || !fileList || !uploadStatus || !uploadPercentage || !progressBar || !uploadedImagesContainer) {
        console.log('Some image upload elements not found, skipping image upload functionality');
        return;
    }

    let uploadedFiles = [];

    // File input change handler
    fileInput.addEventListener('change', function(e) {
        const files = Array.from(e.target.files);
        if (files.length > 0) {
            handleMultipleFileSelect(files);
        }
    });

    // Drag and drop functionality
    const dropzone = document.querySelector('label[for="dropzone-file"]');
    dropzone.addEventListener('dragover', function(e) {
        e.preventDefault();
        dropzone.classList.add('border-blue-500', 'bg-blue-50');
    });

    dropzone.addEventListener('dragleave', function(e) {
        e.preventDefault();
        dropzone.classList.remove('border-blue-500', 'bg-blue-50');
    });

    dropzone.addEventListener('drop', function(e) {
        e.preventDefault();
        dropzone.classList.remove('border-blue-500', 'bg-blue-50');
        
        const files = Array.from(e.dataTransfer.files);
        if (files.length > 0) {
            handleMultipleFileSelect(files);
        }
    });

    // Handle multiple file selection
    function handleMultipleFileSelect(files) {
        // Clear previous files from list only, keep existing uploaded files
        fileList.innerHTML = '';
        
        // Show progress area
        uploadPlaceholder.classList.add('hidden');
        uploadProgress.classList.remove('hidden');
        
        let totalFiles = files.length;
        let processedFiles = 0;
        
        files.forEach((file, index) => {
            handleFileSelect(file, index, () => {
                processedFiles++;
                const progress = Math.round((processedFiles / totalFiles) * 100);
                updateProgress(progress, `Processing ${processedFiles} of ${totalFiles} files...`);
                
                if (processedFiles === totalFiles) {
                    uploadStatus.textContent = 'Upload complete!';
                    setTimeout(() => {
                        uploadProgress.classList.add('hidden');
                        uploadPlaceholder.classList.remove('hidden');
                    }, 2000);
                }
            });
        });
    }

    function handleFileSelect(file, index, callback) {
        // Validate file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/svg+xml'];
        if (!allowedTypes.includes(file.type)) {
            alert(`File ${file.name} is not a valid image type. Please select JPG, PNG, GIF, or SVG files.`);
            return;
        }

        // Validate file size (10MB)
        if (file.size > 10 * 1024 * 1024) {
            alert(`File ${file.name} is too large. File size must be less than 10MB.`);
            return;
        }

        // Create file item element
        const fileItem = document.createElement('div');
        fileItem.classList.add('flex', 'items-center', 'justify-between', 'p-2', 'bg-gray-100', 'rounded-lg', 'mb-2');
        fileItem.innerHTML = `
            <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900 truncate">${file.name}</p>
                <p class="text-xs text-gray-600">${(file.size / 1024).toFixed(2)} KB</p>
            </div>
            <button type="button" class="text-red-600 hover:text-red-800 focus:outline-none transition-colors duration-200" onclick="removeFile(${index})">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                </svg>
            </button>
        `;
        fileList.appendChild(fileItem);

        // Simulate file processing
        setTimeout(() => {
            uploadedFiles.push({
                file: file,
                name: file.name,
                size: file.size,
                preview: URL.createObjectURL(file)
            });
            
            updateUploadedImagesDisplay();
            updateFileInput();
            callback();
        }, 1000);
    }

    function updateFileInput() {
        // Create a DataTransfer object to hold all files
        const dataTransfer = new DataTransfer();
        
        // Add all uploaded files to the DataTransfer
        uploadedFiles.forEach(fileData => {
            dataTransfer.items.add(fileData.file);
        });
        
        // Update the file input with all files
        fileInput.files = dataTransfer.files;
    }

    function updateProgress(percentage, status) {
        uploadPercentage.textContent = `${percentage}%`;
        progressBar.style.width = `${percentage}%`;
        uploadStatus.textContent = status;
    }

    function updateUploadedImagesDisplay() {
        if (uploadedFiles.length === 0) {
            uploadedImagesContainer.innerHTML = `
                <div class="text-center text-gray-500 py-8">
                    <p>No images uploaded yet.</p>
                </div>
            `;
            return;
        }

        const imagesHtml = uploadedFiles.map((fileData, index) => `
            <div class="relative inline-block m-2">
                <img src="${fileData.preview}" alt="${fileData.name}" 
                     class="w-24 h-24 object-cover rounded-lg border border-gray-200 cursor-pointer hover:opacity-75 transition-opacity duration-200"
                     onclick="previewImage('${fileData.preview}', '${fileData.name}')">
                <button type="button" class="absolute -top-2 -right-2 bg-red-500 text-white rounded-full p-1 hover:bg-red-600 transition-colors duration-200"
                        onclick="removeUploadedFile(${index})">
                    <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>
        `).join('');

        uploadedImagesContainer.innerHTML = `
            <div class="flex flex-wrap gap-2">
                ${imagesHtml}
            </div>
        `;
    }

    // Remove file from upload list
    window.removeFile = function(index) {
        const fileItems = fileList.children;
        if (fileItems[index]) {
            fileItems[index].remove();
        }
        
        // If no files left, show placeholder
        if (fileList.children.length === 0) {
            uploadPlaceholder.classList.remove('hidden');
            uploadProgress.classList.add('hidden');
        }
    };

    // Remove uploaded file
    window.removeUploadedFile = function(index) {
        uploadedFiles.splice(index, 1);
        updateUploadedImagesDisplay();
        updateFileInput();
    };

    // Preview image in modal
    window.previewImage = function(imageSrc, imageName) {
        const previewModal = document.createElement('div');
        previewModal.className = 'fixed inset-0 flex items-center justify-center z-50';
        previewModal.id = 'preview-modal-backdrop';
        previewModal.style.backgroundColor = 'rgba(0, 0, 0, 0.5)';
        previewModal.style.backdropFilter = 'blur(10px)';
        previewModal.style.webkitBackdropFilter = 'blur(10px)';
        
        previewModal.innerHTML = `
            <div class="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-full">
                <div class="relative p-6">
                    <button type="button" class="absolute top-3 right-3 text-gray-400 hover:text-gray-600 rounded-full p-1" onclick="closePreviewModal()">
                        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                        </svg>
                    </button>
                    <div class="text-center">
                        <h3 class="text-lg font-semibold text-gray-900 mb-4">${imageName}</h3>
                        <img src="${imageSrc}" alt="${imageName}" class="max-w-full max-h-96 object-contain mx-auto">
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(previewModal);
        
        // Add click event to close modal when clicking backdrop
        previewModal.addEventListener('click', function(event) {
            if (event.target === previewModal) {
                closePreviewModal();
            }
        });
        
        // Add ESC key to close modal
        document.addEventListener('keydown', function(event) {
            if (event.key === 'Escape') {
                closePreviewModal();
            }
        });
    };

    // Global function for closing preview modal
    window.closePreviewModal = function() {
        const modal = document.querySelector('#preview-modal-backdrop');
        if (modal) {
            document.body.removeChild(modal);
        }
    };
});