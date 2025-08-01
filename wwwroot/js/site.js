//Layout
document.addEventListener('DOMContentLoaded', function () {
    var openBtn = document.getElementById('openMenuBtn');
    var closeBtn = document.getElementById('closeMenuBtn');
    var mobileMenu = document.getElementById('mobileMenu');
    if (openBtn && closeBtn && mobileMenu) {
        openBtn.addEventListener('click', function () {
            mobileMenu.classList.remove('hidden');
        });
        closeBtn.addEventListener('click', function () {
            mobileMenu.classList.add('hidden');
        });
    }
    const photoFileInput = document.getElementById('photoFile');
    const fileNameSpan = document.getElementById('fileName1');
    const currentPhotoImg = document.getElementById('currentPhoto');

    if (photoFileInput && fileNameSpan && currentPhotoImg) {

        photoFileInput.addEventListener('change', function (e) {

            const file = e.target.files[0];
            const fileName = file?.name || '';

            fileNameSpan.textContent = fileName;

            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    currentPhotoImg.src = e.target.result;
                };
                reader.readAsDataURL(file);
            }
        });
    } else {
    }
});

//Select Role
document.addEventListener('DOMContentLoaded', function () {
    const urlParams = new URLSearchParams(window.location.search);
    const role = urlParams.get('role');
    if (role) {
        const dropdown = document.getElementById('role-address-icon');
        dropdown.value = role;
    }
});


//SignUpForm
let currentPart = 1;
const totalParts = 2;

const nextBtn = document.getElementById('nextBtn');
const backBtn = document.getElementById('backBtn');
const submitBtn = document.getElementById('submitBtn');
const firstPart = document.getElementById('firstPart');
const secondPart = document.getElementById('secondPart');

function initializeForm() {
    const secondPartErrors = secondPart.querySelectorAll('.text-danger');
    let hasSecondPartErrors = false;

    secondPartErrors.forEach(error => {
        if (error.textContent.trim() !== '') {
            hasSecondPartErrors = true;
        }
    });

    if (hasSecondPartErrors) {
        currentPart = 2;
    }

    updateForm();
}

nextBtn.addEventListener('click', function () {
    if (currentPart < totalParts) {
        const inputs = firstPart.querySelectorAll('input, select, textarea');
        let valid = true;
        for (const input of inputs) {
            if (!input.checkValidity()) {
                input.reportValidity();
                valid = false;
                break;
            }
        }
        if (valid) {
            currentPart++;
            updateForm();
        }
    }
});

backBtn.addEventListener('click', function () {
    if (currentPart > 1) {
        currentPart--;
        updateForm();
    }
});

function updateForm() {
    firstPart.classList.add('hidden');
    secondPart.classList.add('hidden');

    if (currentPart === 1) {
        firstPart.classList.remove('hidden');
        nextBtn.classList.remove('hidden');
        backBtn.classList.add('hidden');
        submitBtn.classList.add('hidden');
    } else if (currentPart === 2) {
        secondPart.classList.remove('hidden');
        nextBtn.classList.add('hidden');
        backBtn.classList.remove('hidden');
        submitBtn.classList.remove('hidden');
    }
}

document.addEventListener('DOMContentLoaded', initializeForm);


//Delete Post
function deletePost() {
    const form = document.getElementById('editPostForm');
    const actionInput = document.createElement('input');
    actionInput.type = 'hidden';
    actionInput.name = 'action';
    actionInput.value = 'delete';
    form.appendChild(actionInput);

    form.submit();
}

//Delete Bid
function deleteBid() {
    const form = document.getElementById('editBidForm');
    const actionInput = document.createElement('input');
    actionInput.type = 'hidden';
    actionInput.name = 'action';
    actionInput.value = 'delete';
    form.appendChild(actionInput);

    form.submit();
}

//Accept Bid
function acceptBid(bidId) {
    const form = document.getElementById('acceptBidForm_' + bidId);
    if (form) {
        form.submit();
    }
}