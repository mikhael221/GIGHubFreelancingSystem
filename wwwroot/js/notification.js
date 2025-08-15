class NotificationManager {
    constructor() {
        this.notificationDropdown = document.getElementById('dropdownNotification');
        this.notificationButton = document.getElementById('dropdownNotificationButton');
        this.notificationBadge = this.notificationButton?.querySelector('.absolute.inline-flex');
        this.notificationsContainer = this.notificationDropdown?.querySelector('.divide-y.divide-gray-100');
        this.connection = null;
        
        this.init();
    }

    async init() {
        if (!this.notificationDropdown || !this.notificationButton) {
            return;
        }

        // Load initial notifications
        await this.loadNotifications();
        
        // Set up event listeners
        this.setupEventListeners();
        
        // Connect to SignalR for real-time updates
        await this.connectToSignalR();
        
        // Keep polling as fallback (but less frequently)
        this.startPolling();
    }

    async connectToSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/mentorshipChatHub')
                .withAutomaticReconnect()
                .build();

            await this.connection.start();
            console.log('NotificationManager: Connected to SignalR');

            // Set up SignalR event handlers
            this.setupSignalREventHandlers();
        } catch (error) {
            console.error('NotificationManager: Failed to connect to SignalR:', error);
        }
    }

    setupSignalREventHandlers() {
        if (!this.connection) return;

        // Handle real-time notifications
        this.connection.on('ReceiveNotification', (notification) => {
            console.log('Received real-time notification:', notification);
            this.addNotificationToDropdown(notification);
            this.updateNotificationBadge(this.getCurrentUnreadCount() + 1);
        });

        // Handle notification count updates
        this.connection.on('UpdateNotificationCount', (count) => {
            console.log('Updated notification count:', count);
            this.updateNotificationBadge(count);
        });

        // Handle connection state changes
        this.connection.onclose(() => {
            console.log('NotificationManager: SignalR connection closed');
        });

        this.connection.onreconnecting(() => {
            console.log('NotificationManager: SignalR reconnecting...');
        });

        this.connection.onreconnected(() => {
            console.log('NotificationManager: SignalR reconnected');
        });
    }

    addNotificationToDropdown(notification) {
        if (!this.notificationsContainer) return;

        // Create notification element
        const notificationElement = this.createNotificationElement(notification);
        
        // Insert at the top of the container
        if (this.notificationsContainer.firstChild) {
            this.notificationsContainer.insertBefore(notificationElement, this.notificationsContainer.firstChild);
        } else {
            this.notificationsContainer.appendChild(notificationElement);
        }

        // Remove "No notifications yet" message if it exists
        const noNotificationsMsg = this.notificationsContainer.querySelector('.text-center.text-gray-500');
        if (noNotificationsMsg) {
            noNotificationsMsg.remove();
        }

        // Add animation for new notification
        notificationElement.style.animation = 'slideInDown 0.3s ease-out';
    }

    getCurrentUnreadCount() {
        if (!this.notificationBadge) return 0;
        const countText = this.notificationBadge.textContent;
        if (countText === '99+') return 99;
        return parseInt(countText) || 0;
    }

    setupEventListeners() {
        // Remove the old "View all" link event listener since we now handle it dynamically
        // The new "View All" button is created dynamically and has its own event listener
    }

    async loadNotifications() {
        try {
            const response = await fetch('/Notification/GetNotifications');
            if (response.ok) {
                const notifications = await response.json();
                this.updateNotificationDropdown(notifications);
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
        }

        // Update unread count
        await this.updateUnreadCount();
    }

    async updateUnreadCount() {
        try {
            const response = await fetch('/Notification/GetUnreadCount');
            if (response.ok) {
                const data = await response.json();
                this.updateNotificationBadge(data.count);
            }
        } catch (error) {
            console.error('Error updating unread count:', error);
        }
    }

    updateNotificationDropdown(notifications) {
        if (!this.notificationsContainer) return;

        // Clear existing notifications
        this.notificationsContainer.innerHTML = '';

        if (notifications.length === 0) {
            this.notificationsContainer.innerHTML = `
                <div class="px-4 py-3 text-center text-gray-500 text-sm">
                    No notifications yet
                </div>
            `;
            return;
        }

        // Store all notifications for potential expansion
        this.allNotifications = notifications;

        // Show only the first 4 notifications initially
        const initialNotifications = notifications.slice(0, 4);
        
        initialNotifications.forEach(notification => {
            const notificationElement = this.createNotificationElement(notification);
            this.notificationsContainer.appendChild(notificationElement);
        });

        // Add "View All" button if there are more than 4 notifications
        if (notifications.length > 4) {
            const viewAllButton = document.createElement('div');
            viewAllButton.className = 'px-4 py-2 text-center text-sm font-medium text-blue-600 bg-gray-50 border-t border-gray-100 cursor-pointer hover:bg-gray-100 sticky bottom-0';
            viewAllButton.innerHTML = `
                <div class="flex items-center justify-center gap-1">
                    <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd"></path>
                    </svg>
                    View All (${notifications.length})
                </div>
            `;
            
            // Add click event to expand notifications
            viewAllButton.addEventListener('click', () => {
                this.expandNotifications();
            });
            
            this.notificationsContainer.appendChild(viewAllButton);
            this.viewAllButton = viewAllButton;
        }
    }

    expandNotifications() {
        if (!this.notificationsContainer || !this.allNotifications) return;

        // Clear existing notifications
        this.notificationsContainer.innerHTML = '';

        // Ensure the container has proper scrolling when expanded
        this.notificationsContainer.style.maxHeight = '400px'; // Increased height for expanded view
        this.notificationsContainer.style.overflowY = 'auto';

        // Show all notifications
        this.allNotifications.forEach(notification => {
            const notificationElement = this.createNotificationElement(notification);
            this.notificationsContainer.appendChild(notificationElement);
        });

        // Add "Show Less" button
        const showLessButton = document.createElement('div');
        showLessButton.className = 'px-4 py-2 text-center text-sm font-medium text-blue-600 bg-gray-50 border-t border-gray-100 cursor-pointer hover:bg-gray-100 sticky bottom-0';
        showLessButton.innerHTML = `
            <div class="flex items-center justify-center gap-1">
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M14.707 12.707a1 1 0 01-1.414 0L10 9.414l-3.293 3.293a1 1 0 01-1.414-1.414l4-4a1 1 0 011.414 0l4 4a1 1 0 010 1.414z" clip-rule="evenodd"></path>
                </svg>
                Show Less
            </div>
        `;
        
        // Add click event to collapse notifications
        showLessButton.addEventListener('click', () => {
            this.collapseNotifications();
        });
        
        this.notificationsContainer.appendChild(showLessButton);
        this.showLessButton = showLessButton;
    }

    collapseNotifications() {
        if (!this.notificationsContainer || !this.allNotifications) return;

        // Clear existing notifications
        this.notificationsContainer.innerHTML = '';

        // Reset container styles back to original state
        this.notificationsContainer.style.maxHeight = ''; // Reset to CSS default
        this.notificationsContainer.style.overflowY = ''; // Reset to CSS default

        // Show only the first 4 notifications
        const initialNotifications = this.allNotifications.slice(0, 4);
        
        initialNotifications.forEach(notification => {
            const notificationElement = this.createNotificationElement(notification);
            this.notificationsContainer.appendChild(notificationElement);
        });

        // Add "View All" button again
        if (this.allNotifications.length > 4) {
            const viewAllButton = document.createElement('div');
            viewAllButton.className = 'px-4 py-2 text-center text-sm font-medium text-blue-600 bg-gray-50 border-t border-gray-100 cursor-pointer hover:bg-gray-100 sticky bottom-0';
            viewAllButton.innerHTML = `
                <div class="flex items-center justify-center gap-1">
                    <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd"></path>
                    </svg>
                    View All (${this.allNotifications.length})
                </div>
            `;
            
            // Add click event to expand notifications
            viewAllButton.addEventListener('click', () => {
                this.expandNotifications();
            });
            
            this.notificationsContainer.appendChild(viewAllButton);
            this.viewAllButton = viewAllButton;
        }
    }

    createNotificationElement(notification) {
        const div = document.createElement('div');
        div.className = 'notification-item';
        
        const isUnread = !notification.isRead;
        const unreadClass = isUnread ? 'bg-blue-50' : '';
        
        // All notification icons now display without blue background
        const iconContainerClass = 'w-11 h-11 flex items-center justify-center';
        
        div.innerHTML = `
            <a href="${notification.relatedUrl || '#'}" class="flex px-4 py-3 hover:bg-gray-100 ${unreadClass}" data-notification-id="${notification.id}">
                <div class="shrink-0 relative">
                    <div class="${iconContainerClass}">
                        ${notification.iconSvg || this.getDefaultIcon(notification.type)}
                    </div>
                    ${isUnread ? '<div class="absolute flex items-center justify-center w-3 h-3 ms-8 -mt-1 bg-blue-600 border border-white rounded-full"></div>' : ''}
                </div>
                <div class="w-full ps-3">
                    <div class="text-gray-500 text-sm mb-1.5">
                        <span class="font-semibold text-gray-900">${notification.title}</span>
                    </div>
                    <div class="text-gray-500 text-sm mb-1.5">${notification.message}</div>
                    <div class="text-xs text-blue-600">${this.formatTimeAgo(notification.createdAt)}</div>
                </div>
            </a>
        `;

        // Add click event to mark as read
        const link = div.querySelector('a');
        link.addEventListener('click', async (e) => {
            if (!notification.isRead) {
                await this.markAsRead(notification.id);
            }
        });

        return div;
    }

    getDefaultIcon(type) {
        switch (type) {
            case 'registration':
                return '<svg class="w-6 h-6 text-blue-600" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 20 20"><path d="M10 0a10 10 0 1 0 10 10A10.009 10.009 0 0 0 10 0Zm3.536 7.707-4.243 4.243a1 1 0 0 1-1.414 0l-2.121-2.121a1 1 0 0 1 1.414-1.414L9 9.586l3.536-3.536a1 1 0 0 1 1.414 1.414Z"/></svg>';
            case 'message':
                return '<svg class="w-6 h-6 text-green-600" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"/><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"/></svg>';
            case 'bid':
                return '<svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="w-6 h-6"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path fill-rule="evenodd" clip-rule="evenodd" d="M17.4964 21.9284C17.844 21.7894 18.1491 21.6495 18.4116 21.5176C18.9328 22.4046 19.8969 23 21 23C22.6569 23 24 21.6568 24 20V14C24 12.3431 22.6569 11 21 11C19.5981 11 18.4208 11.9616 18.0917 13.2612C17.8059 13.3614 17.5176 13.4549 17.2253 13.5384C16.3793 13.7801 15.3603 13.9999 14.5 13.9999C13.2254 13.9999 10.942 13.5353 9.62034 13.2364C8.61831 13.0098 7.58908 13.5704 7.25848 14.5622L6.86313 15.7483C5.75472 15.335 4.41275 14.6642 3.47619 14.1674C2.42859 13.6117 1.09699 14.0649 0.644722 15.1956L0.329309 15.9841C0.0210913 16.7546 0.215635 17.6654 0.890813 18.2217C1.66307 18.8581 3.1914 20.0378 5.06434 21.063C6.91913 22.0782 9.21562 22.9999 11.5 22.9999C14.1367 22.9999 16.1374 22.472 17.4964 21.9284ZM20 20C20 20.5523 20.4477 21 21 21C21.5523 21 22 20.5523 22 20V14C22 13.4477 21.5523 13 21 13C20.4477 13 20 13.4477 20 14V20ZM14.5 15.9999C12.9615 15.9999 10.4534 15.4753 9.17918 15.1872C9.17918 15.1872 8.84483 16.1278 8.7959 16.2745L12.6465 17.2776C13.1084 17.3979 13.372 17.8839 13.2211 18.3367C13.0935 18.7194 12.7092 18.9536 12.3114 18.8865C11.0903 18.6805 8.55235 18.2299 7.25848 17.8365C5.51594 17.3066 3.71083 16.5559 2.53894 15.9342C2.53894 15.9342 2.22946 16.6189 2.19506 16.7049C2.92373 17.3031 4.32792 18.3799 6.0246 19.3086C7.76488 20.2611 9.70942 20.9999 11.5 20.9999C15.023 20.9999 17.1768 19.9555 18 19.465V15.3956C16.8681 15.7339 15.6865 15.9999 14.5 15.9999Z" fill="currentColor"></path> <path d="M12 1C11.4477 1 11 1.44772 11 2V7.58564L9.7071 6.29278C9.3166 5.9024 8.68342 5.9024 8.29292 6.29278C7.90235 6.68341 7.90235 7.31646 8.29292 7.70709L11.292 10.7063C11.6823 11.0965 12.3149 11.0968 12.7055 10.707L15.705 7.71368C16.0955 7.3233 16.0955 6.69 15.705 6.29962C15.3145 5.90899 14.6813 5.90899 14.2908 6.29962L13 7.59034V2C13 1.44772 12.5523 1 12 1Z" fill="currentColor"></path> </g></svg>';
            case 'bid_accepted':
                return '<svg class="w-6 h-6" fill="currentColor" version="1.1" id="Capa_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 47.001 47.001" xml:space="preserve"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <g> <g id="Layer_1_120_"> <g> <g> <path d="M31.736,10.307c-0.111-0.112-0.249-0.193-0.398-0.24l-8.975-2.818c-3.589-1.127-5.924,0.839-6.553,1.47 c-0.367,0.367-0.648,0.754-0.792,1.091l-3.998,9.404c-0.229,0.538-0.151,1.255,0.208,1.97c0.514,1.021,1.44,1.757,2.547,2.022 c1.239,0.297,2.524-0.106,3.53-1.111c0.263-0.263,0.487-0.553,0.619-0.799l1.344-2.493c0.221-0.413,0.542-0.841,0.88-1.179 c1.153-1.154,1.701-0.626,1.934-0.402c2.011,1.941,12.554,12.529,12.554,12.529c0.375,0.375,0.297,1.086-0.172,1.554 c-0.468,0.467-1.18,0.547-1.554,0.174l-2.962-2.961c-0.382-0.383-0.998-0.383-1.38,0c-0.382,0.379-0.382,0.998,0,1.379 l2.962,2.963c0.374,0.373,0.296,1.084-0.172,1.551c-0.468,0.469-1.181,0.547-1.553,0.174l-2.963-2.961 c-0.382-0.382-1-0.382-1.38,0c-0.382,0.379-0.382,0.998,0,1.38l2.962,2.962c0.374,0.374,0.293,1.085-0.174,1.553 c-0.467,0.467-1.178,0.547-1.553,0.172l-2.962-2.961c-0.38-0.381-0.999-0.381-1.38,0c-0.38,0.381-0.38,1,0,1.379l2.962,2.963 c0.375,0.375,0.295,1.086-0.172,1.554c-0.47,0.468-1.181,0.547-1.554,0.173l-3.606-3.609c0.515-0.774,0.375-1.897-0.389-2.664 c-0.856-0.855-2.173-0.934-2.935-0.17c0.762-0.763,0.687-2.078-0.171-2.935c-0.858-0.856-2.172-0.935-2.934-0.173 c0.762-0.762,0.685-2.076-0.174-2.932c-0.856-0.858-2.17-0.936-2.934-0.174c0.764-0.762,0.685-2.076-0.172-2.935 c-0.802-0.802-1.997-0.911-2.774-0.3l-5.839-5.839c-0.381-0.382-1-0.382-1.381,0c-0.38,0.38-0.38,0.999,0,1.381l5.824,5.823 l-1.727,1.727c-0.762,0.761-0.685,2.075,0.174,2.934c0.856,0.856,2.17,0.935,2.933,0.172c-0.763,0.763-0.685,2.076,0.173,2.934 c0.856,0.855,2.171,0.936,2.934,0.173c-0.763,0.763-0.686,2.076,0.172,2.933c0.858,0.858,2.172,0.936,2.934,0.174 c-0.762,0.761-0.685,2.074,0.173,2.933c0.857,0.856,2.17,0.935,2.934,0.172l1.824-1.823l3.581,3.58 c1.143,1.143,3.076,1.063,4.314-0.173c0.603-0.603,0.925-1.373,0.97-2.135c0.762-0.045,1.533-0.368,2.135-0.972 c0.604-0.603,0.928-1.373,0.974-2.135c0.761-0.045,1.529-0.367,2.135-0.971c0.603-0.604,0.926-1.373,0.97-2.136 c0.763-0.044,1.533-0.366,2.137-0.972c1.236-1.236,1.312-3.172,0.172-4.313l-1.51-1.511l6.2-6.199 c0.381-0.38,0.381-0.999,0-1.38L31.736,10.307z"></path> </g> <g> <path d="M46.43,12.489l-7.901-7.901c-0.762-0.763-1.999-0.763-2.762,0l-2.762,2.76c-0.762,0.762-0.762,1.999,0,2.761 l7.902,7.903c0.763,0.762,2,0.762,2.762,0l2.761-2.761C47.191,14.488,47.191,13.251,46.43,12.489z M42.283,16.513 c-0.884,0-1.602-0.717-1.602-1.602c0-0.885,0.718-1.603,1.602-1.603c0.885,0,1.603,0.717,1.603,1.603 C43.885,15.795,43.168,16.513,42.283,16.513z"></path> </g> </g> </g> </g> </g></svg>';
            case 'mentorship_request':
                return '<svg fill="#000000" viewBox="0 0 16 16" id="request-new-16px" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path id="Path_46" data-name="Path 46" d="M-17,11a2,2,0,0,0,2-2,2,2,0,0,0-2-2,2,2,0,0,0-2,2A2,2,0,0,0-17,11Zm0-3a1,1,0,0,1,1,1,1,1,0,0,1-1,1,1,1,0,0,1-1-1A1,1,0,0,1-17,8Zm2.5,4h-5A2.5,2.5,0,0,0-22,14.5,1.5,1.5,0,0,0-20.5,16h7A1.5,1.5,0,0,0-12,14.5,2.5,2.5,0,0,0-14.5,12Zm1,3h-7a.5.5,0,0,1-.5-.5A1.5,1.5,0,0,1-19.5,13h5A1.5,1.5,0,0,1-13,14.5.5.5,0,0,1-13.5,15ZM-6,2.5v5A2.5,2.5,0,0,1-8.5,10h-2.793l-1.853,1.854A.5.5,0,0,1-13.5,12a.489.489,0,0,1-.191-.038A.5.5,0,0,1-14,11.5v-2a.5.5,0,0,1,.5-.5.5.5,0,0,1,.5.5v.793l1.146-1.147A.5.5,0,0,1-11.5,9h3A1.5,1.5,0,0,0-7,7.5v-5A1.5,1.5,0,0,0-8.5,1h-7A1.5,1.5,0,0,0-17,2.5v3a.5.5,0,0,1,.5.5.5.5,0,0,1-.5-.5v-3A2.5,2.5,0,0,1-15.5,0h7A2.5,2.5,0,0,1-6,2.5ZM-11.5,2V4.5H-9a.5.5,0,0,1,.5.5.5.5,0,0,1-.5.5h-2.5V8a.5.5,0,0,1,.5.5.5.5,0,0,1-.5-.5V5.5H-15a.5.5,0,0,1-.5-.5.5.5,0,0,1,.5-.5h2.5V2a.5.5,0,0,1,.5-.5A.5.5,0,0,1-11.5,2Z" transform="translate(22)"></path> </g></svg>';
            case 'mentorship_accepted':
                return '<svg class="w-6 h-6 text-green-600" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 20 20"><path d="M10 0a10 10 0 1 0 10 10A10.009 10.009 0 0 0 10 0Zm3.536 7.707-4.243 4.243a1 1 0 0 1-1.414 0l-2.121-2.121a1 1 0 0 1 1.414-1.414L9 9.586l3.536-3.536a1 1 0 0 1 1.414 1.414Z"/></svg>';
            case 'mentorship_declined':
                return '<svg class="w-6 h-6 text-red-600" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 20 20"><path d="M10 0a10 10 0 1 0 10 10A10.009 10.009 0 0 0 10 0Zm3.536 7.707-4.243 4.243a1 1 0 0 1-1.414 0l-2.121-2.121a1 1 0 0 1 1.414-1.414L9 9.586l3.536-3.536a1 1 0 0 1 1.414 1.414Z"/></svg>';
            case 'session_proposal':
                return '<svg class="w-6 h-6 text-blue-600" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M3 9H21M12 18V12M15 15.001L9 15M7 3V5M17 3V5M6.2 21H17.8C18.9201 21 19.4802 21 19.908 20.782C20.2843 20.5903 20.5903 20.2843 20.782 19.908C21 19.4802 21 18.9201 21 17.8V8.2C21 7.07989 21 6.51984 20.782 6.09202C20.5903 5.71569 20.2843 5.40973 19.908 5.21799C19.4802 5 18.9201 5 17.8 5H6.2C5.0799 5 4.51984 5 4.09202 5.21799C3.71569 5.40973 3.40973 5.71569 3.21799 6.09202C3 6.51984 3 7.07989 3 8.2V17.8C3 18.9201 3 19.4802 3.21799 19.908C3.40973 20.2843 3.71569 20.5903 4.09202 20.782C4.51984 21 5.07989 21 6.2 21Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"></path> </g></svg>';
            case 'session_accepted':
                return '<svg class="w-6 h-6 text-green-600" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M3 9H21M9 15L11 17L15 13M7 3V5M17 3V5M6.2 21H17.8C18.9201 21 19.4802 21 19.908 20.782C20.2843 20.5903 20.5903 20.2843 20.782 19.908C21 19.4802 21 18.9201 21 17.8V8.2C21 7.07989 21 6.51984 20.782 6.09202C20.5903 5.71569 20.2843 5.40973 19.908 5.21799C19.4802 5 18.9201 5 17.8 5H6.2C5.0799 5 4.51984 5 4.09202 5.21799C3.71569 5.40973 3.40973 5.71569 3.21799 6.09202C3 6.51984 3 7.07989 3 8.2V17.8C3 18.9201 3 19.4802 3.21799 19.908C3.40973 20.2843 3.71569 20.5903 4.09202 20.782C4.51984 21 5.07989 21 6.2 21Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"></path> </g></svg>';
            case 'session_declined':
                return '<svg class="w-6 h-6 text-red-600" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M10 13L14 17M14 13L10 17M3 9H21M7 3V5M17 3V5M6.2 21H17.8C18.9201 21 19.4802 21 19.908 20.782C20.2843 20.5903 20.5903 20.2843 20.782 19.908C21 19.4802 21 18.9201 21 17.8V8.2C21 7.07989 21 6.51984 20.782 6.09202C20.5903 5.71569 20.2843 5.40973 19.908 5.21799C19.4802 5 18.9201 5 17.8 5H6.2C5.0799 5 4.51984 5 4.09202 5.21799C3.71569 5.40973 3.40973 5.71569 3.21799 6.09202C3 6.51984 3 7.07989 3 8.2V17.8C3 18.9201 3 19.4802 3.21799 19.908C3.40973 20.2843 3.71569 20.5903 4.09202 20.782C4.51984 21 5.07989 21 6.2 21Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"></path> </g></svg>';
            case 'mentor_review':
                return '<svg class="w-6 h-6 text-purple-600" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M16 1C17.6569 1 19 2.34315 19 4C19 4.55228 18.5523 5 18 5C17.4477 5 17 4.55228 17 4C17 3.44772 16.5523 3 16 3H4C3.44772 3 3 3.44772 3 4V20C3 20.5523 3.44772 21 4 21H16C16.5523 21 17 20.5523 17 20V19C17 18.4477 17.4477 18 18 18C18.5523 18 19 18.4477 19 19V20C19 21.6569 17.6569 23 16 23H4C2.34315 23 1 21.6569 1 20V4C1 2.34315 2.34315 1 4 1H16Z" fill="currentColor"></path> <path fill-rule="evenodd" clip-rule="evenodd" d="M20.7991 8.20087C20.4993 7.90104 20.0132 7.90104 19.7133 8.20087L11.9166 15.9977C11.7692 16.145 11.6715 16.3348 11.6373 16.5404L11.4728 17.5272L12.4596 17.3627C12.6652 17.3285 12.855 17.2308 13.0023 17.0835L20.7991 9.28666C21.099 8.98682 21.099 8.5007 20.7991 8.20087ZM18.2991 6.78666C19.38 5.70578 21.1325 5.70577 22.2134 6.78665C23.2942 7.86754 23.2942 9.61999 22.2134 10.7009L14.4166 18.4977C13.9744 18.9398 13.4052 19.2327 12.7884 19.3355L11.8016 19.5C10.448 19.7256 9.2744 18.5521 9.50001 17.1984L9.66448 16.2116C9.76728 15.5948 10.0602 15.0256 10.5023 14.5834L18.2991 6.78666Z" fill="currentColor"></path> <path d="M5 7C5 6.44772 5.44772 6 6 6H14C14.5523 6 15 6.44772 15 7C15 7.55228 14.5523 8 14 8H6C5.44772 8 5 7.55228 5 7Z" fill="currentColor"></path> <path d="M5 11C5 10.4477 5.44772 10 6 10H10C10.5523 10 11 10.4477 11 11C11 11.5523 10.5523 12 10 12H6C5.44772 12 5 11.5523 5 11Z" fill="currentColor"></path> <path d="M5 15C5 14.4477 5.44772 14 6 14H7C7.55228 14 8 14.4477 8 15C8 15.5523 7.55228 16 7 16H6C5.44772 16 5 15.5523 5 15Z" fill="currentColor"></path> </g></svg>';
            case 'contract_created':
                return '<svg class="w-6 h-6 text-blue-600" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" fill="currentColor"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"><path d="M182.52 146.2h585.14v256h73.15V73.06H109.38v877.71h256v-73.14H182.52z" fill="currentColor"></path><path d="M255.67 219.34h438.86v73.14H255.67zM255.67 365.63h365.71v73.14H255.67zM255.67 511.91H475.1v73.14H255.67zM775.22 458.24L439.04 794.42l-0.52 154.64 155.68 0.52L930.38 613.4 775.22 458.24z m51.72 155.16l-25.43 25.43-51.73-51.72 25.44-25.44 51.72 51.73z m-77.14 77.15L620.58 819.77l-51.72-51.72 129.22-129.22 51.72 51.72zM511.91 876.16l0.17-51.34 5.06-5.06 51.72 51.72-4.85 4.85-52.1-0.17z" fill="currentColor"></path></g></svg>';
            case 'Contract':
                return '<svg class="w-6 h-6 text-green-600" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" fill="currentColor"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"><path d="M182.52 146.2h585.14v402.28h73.15V73.06H109.38v877.71h402.28v-73.14H182.52z" fill="currentColor"></path><path d="M255.67 219.34h438.86v73.14H255.67zM255.67 365.63h365.71v73.14H255.67zM255.67 511.91H475.1v73.14H255.67zM731.02 585.06c-100.99 0-182.86 81.87-182.86 182.86s81.87 182.86 182.86 182.86 182.86-81.87 182.86-182.86-81.87-182.86-182.86-182.86z m0 292.57c-60.5 0-109.71-49.22-109.71-109.71 0-60.5 49.22-109.71 109.71-109.71 60.5 0 109.71 49.22 109.71 109.71 0 60.49-49.22 109.71-109.71 109.71z" fill="currentColor"></path><path d="M717.88 777.65l-42.55-38.13-36.61 40.86 84.02 75.27 102.98-118.47-41.39-36z" fill="currentColor"></path></g></svg>';
            default:
                return '<svg class="w-6 h-6 text-gray-600" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 20 20"><path d="M10 0a10 10 0 1 0 10 10A10.009 10.009 0 0 0 10 0Zm3.536 7.707-4.243 4.243a1 1 0 0 1-1.414 0l-2.121-2.121a1 1 0 0 1 1.414-1.414L9 9.586l3.536-3.536a1 1 0 0 1 1.414 1.414Z"/></svg>';
        }
    }

    formatTimeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffInSeconds = Math.floor((now - date) / 1000);

        if (diffInSeconds < 60) {
            return 'just now';
        } else if (diffInSeconds < 3600) {
            const minutes = Math.floor(diffInSeconds / 60);
            return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
        } else if (diffInSeconds < 86400) {
            const hours = Math.floor(diffInSeconds / 3600);
            return `${hours} hour${hours > 1 ? 's' : ''} ago`;
        } else {
            const days = Math.floor(diffInSeconds / 86400);
            return `${days} day${days > 1 ? 's' : ''} ago`;
        }
    }

    updateNotificationBadge(count) {
        if (!this.notificationBadge) return;

        if (count > 0) {
            this.notificationBadge.style.display = 'block';
            this.notificationBadge.textContent = count > 99 ? '99+' : count.toString();
        } else {
            this.notificationBadge.style.display = 'none';
        }
    }

    async markAsRead(notificationId) {
        try {
            const response = await fetch(`/Notification/MarkAsRead?notificationId=${notificationId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (response.ok) {
                // Update the notification item to remove unread styling
                const notificationItem = document.querySelector(`[data-notification-id="${notificationId}"]`);
                if (notificationItem) {
                    notificationItem.classList.remove('bg-blue-50');
                    const unreadDot = notificationItem.querySelector('.absolute');
                    if (unreadDot) {
                        unreadDot.remove();
                    }
                }
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    async markAllAsRead() {
        try {
            const response = await fetch('/Notification/MarkAllAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (response.ok) {
                // Remove unread styling from all notifications
                const unreadItems = this.notificationsContainer?.querySelectorAll('.bg-blue-50');
                unreadItems?.forEach(item => {
                    item.classList.remove('bg-blue-50');
                });

                const unreadDots = this.notificationsContainer?.querySelectorAll('.absolute');
                unreadDots?.forEach(dot => {
                    dot.remove();
                });
            }
        } catch (error) {
            console.error('Error marking all notifications as read:', error);
        }
    }

    startPolling() {
        // Poll for new notifications every 60 seconds as fallback (less frequent since we have real-time updates)
        setInterval(async () => {
            await this.updateUnreadCount();
        }, 60000);
    }
}

// Initialize notification manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.notificationManager = new NotificationManager();
});
