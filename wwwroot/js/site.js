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
});

//ProjectPosting
document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('projectForm');
    var countElem = document.getElementById('totalProject');

    if (form && countElem) {
        form.addEventListener('submit', function (e) {
            // Increment the count
            var current = parseInt(countElem.textContent, 10) || 0;
            countElem.textContent = current + 1;
        });
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

nextBtn.addEventListener('click', function () {
	if (currentPart < totalParts) {
		if (validateCurrentPart()) {
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

function validateCurrentPart() {
	if (currentPart === 1) {
		const firstName = document.getElementById('firstname-address-icon').value;
		const lastName = document.getElementById('lastname-address-icon').value;

		if (!firstName || !lastName) {
			alert('Please fill in all required fields.');
			return false;
		}
	}
	return true;
}

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