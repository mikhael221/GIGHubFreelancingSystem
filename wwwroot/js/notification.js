class NotificationManager {
    constructor() {
        this.notificationDropdown = document.getElementById('dropdownNotification');
        this.notificationButton = document.getElementById('dropdownNotificationButton');
        this.notificationBadge = this.notificationButton?.querySelector('.absolute');
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
                <div class="shrink-0">
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
                return '<svg fill="currentColor" height="24px" width="24px" version="1.1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><g><g><path d="M475.542,203.546c-15.705-15.707-38.776-18.531-57.022-9.796L296.42,71.648c8.866-18.614,5.615-41.609-9.775-56.999 c-19.528-19.531-51.307-19.531-70.837,0L144.97,85.486c-19.529,19.529-19.529,51.307,0,70.836 c15.351,15.353,38.31,18.678,56.999,9.775l25.645,25.645L14.902,404.454c-19.575,19.574-19.578,51.259,0,70.836 c19.575,19.576,51.259,19.579,70.837,0l212.712-212.711l25.642,25.641c-8.868,18.615-5.617,41.609,9.774,57 c9.46,9.46,22.039,14.672,35.419,14.672s25.957-5.21,35.418-14.672l70.837-70.837 C495.072,254.853,495.072,223.077,475.542,203.546z M192.196,132.71c-6.51,6.509-17.103,6.507-23.613,0 c-6.509-6.511-6.509-17.102,0-23.612l70.837-70.837c6.509-6.509,17.1-6.512,23.612,0c6.51,6.51,6.51,17.102,0.001,23.612 L192.196,132.71z M62.127,451.676c-6.526,6.525-17.086,6.526-23.612,0c-6.525-6.525-6.526-17.087,0-23.612l212.712-212.712 l23.612,23.613L62.127,451.676z M227.614,144.516l11.805-11.807l35.419-35.419L392.9,215.353l-47.224,47.225L227.614,144.516z M451.931,250.772l-70.837,70.837c-6.526,6.526-17.086,6.526-23.612,0c-6.51-6.51-6.51-17.103,0-23.613l70.838-70.837 c6.524-6.526,17.086-6.525,23.611,0C458.457,233.684,458.457,244.245,451.931,250.772z"></path></g></g><g><g><path d="M461.691,411.822H328.12c-27.619,0-50.089,22.47-50.089,50.089v33.393c0,9.221,7.476,16.696,16.696,16.696h200.357 c9.221,0,16.696-7.476,16.696-16.696v-33.393C511.781,434.292,489.311,411.822,461.691,411.822z M478.388,478.607H311.424v-16.696 c0-9.206,7.49-16.696,16.696-16.696h133.571c9.206,0,16.696,7.49,16.696,16.696V478.607z"></path></g></g></g></svg>';
            case 'bid_accepted':
                return '<svg height="24px" width="24px" version="1.1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" fill="currentColor"><g><path d="M386.234,78.954l-49.861,49.861l4.635,4.814c-3.302,2.667-6.644,5.269-9.888,7.579 c-5.236,3.74-10.262,6.74-13.669,8.147c-0.22,0.098-1.146,0.358-2.984,0.35c-2.936,0.032-7.847-0.716-13.864-1.634 c-6.042-0.911-13.27-1.902-21.417-1.911c-7.108-0.024-14.946,0.838-23.199,3.106c-8.236-2.26-16.059-3.106-23.149-3.097 c-8.156,0-15.384,0.992-21.426,1.911c-6.017,0.918-10.92,1.667-13.855,1.634c-1.846,0.016-2.765-0.26-2.952-0.35 c-3.635-1.488-9.116-4.806-14.758-8.912c-2.919-2.122-5.895-4.448-8.847-6.838l4.627-4.798l-49.861-49.861L0,209.216l49.861,49.861 l14.595-15.116l32.216,28.117c0.902,5.391,2.602,10.481,4.562,15.312c1.276,3.074,2.675,6.033,4.107,8.814 c-3.115,5.156-4.814,11.043-4.798,17.011c-0.008,5.92,1.618,11.969,4.96,17.344c5.082,8.172,13.222,13.311,22.011,14.937 c0.05,5.814,1.66,11.742,4.936,17.011c6.221,10.002,16.987,15.547,27.964,15.53c0.423,0,0.854-0.065,1.276-0.081 c0.553,4.651,2.082,9.286,4.7,13.498c6.221,10.001,16.994,15.547,27.955,15.53c1.09,0,2.172-0.114,3.269-0.22 c0.52,4.188,1.911,8.367,4.261,12.14c5.668,9.108,15.474,14.165,25.45,14.141c5.383,0.008,10.896-1.456,15.791-4.497l14.946-9.301 c2.944,1.902,6.033,3.577,9.277,4.943c6.042,2.545,12.571,4.106,19.361,4.114c7.448,0.033,15.311-2.008,22.076-6.765 c4.407-3.073,8.139-7.286,11.196-12.253c0.472,0.016,0.854,0.098,1.342,0.106c3.098,0,6.497-0.464,9.864-1.513 c3.39-1.032,6.757-2.578,10.351-4.634c8.57-4.83,13.709-11.937,16.206-18.027c0.553-1.333,0.951-2.586,1.309-3.797 c2.074-0.658,4.058-1.464,5.854-2.504c6.627-3.814,10.945-9.611,13.604-15.433c2.667-5.88,3.838-11.986,3.854-17.856 c0-0.578-0.073-1.139-0.098-1.708c4.334-1.772,8.228-4.236,11.546-7.139c3.399-2.977,6.253-6.44,8.375-10.392 c2.115-3.935,3.53-8.44,3.538-13.375c0.008-3.212-0.651-6.595-2.09-9.766c-0.464-1.008-1.122-1.935-1.724-2.87 c2.53-4.53,5.106-9.635,7.22-15.148c1.586-4.172,2.92-8.546,3.684-13.141l33.59-29.313l15.742,16.303L512,209.216L386.234,78.954z M138.744,320.428l-0.578,0.366c-1.504,0.928-3.082,1.342-4.708,1.35c-3.017-0.016-5.912-1.488-7.611-4.228 c-0.927-1.504-1.35-3.09-1.35-4.7c0.016-3.033,1.48-5.912,4.212-7.611l19.751-12.27c1.504-0.935,3.082-1.341,4.691-1.358 c3.033,0.032,5.912,1.48,7.62,4.236l10.172-6.318l-10.172,6.326c0.927,1.496,1.341,3.073,1.35,4.684 c-0.017,2.114-0.773,4.131-2.138,5.757L143.02,317.2C141.476,318.159,140.069,319.273,138.744,320.428z M165.113,352.733 c-1.504,0.927-3.082,1.35-4.7,1.35c-3.025-0.016-5.919-1.48-7.619-4.22c-0.928-1.504-1.342-3.098-1.35-4.708 c0.007-2.097,0.764-4.114,2.122-5.724l16.97-10.562c1.537-0.952,2.928-2.057,4.253-3.212l0.618-0.374 c1.504-0.935,3.09-1.35,4.692-1.35c3.041,0.016,5.911,1.48,7.618,4.228c0.927,1.504,1.342,3.073,1.35,4.692 c-0.016,3.016-1.471,5.911-4.236,7.627L165.113,352.733z M218.771,369.419l-19.726,12.262c-1.496,0.928-3.074,1.35-4.7,1.35 c-3.017-0.016-5.912-1.48-7.611-4.22c-0.927-1.512-1.341-3.09-1.35-4.708c0.025-3.033,1.48-5.903,4.212-7.611l19.751-12.27 c1.504-0.935,3.082-1.341,4.691-1.341c3.033,0.007,5.912,1.471,7.62,4.22c0.926,1.505,1.341,3.082,1.35,4.7 C222.991,364.816,221.536,367.694,218.771,369.419z M249.353,389.421c0.625,1,0.894,2.041,0.902,3.13 c-0.016,2.025-0.984,3.952-2.846,5.115l-16.938,10.521c-1.008,0.634-2.049,0.902-3.147,0.902c-2.032-0.008-3.959-0.992-5.106-2.829 c-0.626-1.017-0.895-2.058-0.903-3.146c0.024-2.042,0.992-3.953,2.821-5.091h0.008l16.954-10.538 c1.033-0.642,2.066-0.911,3.147-0.911c2.041,0.016,3.952,0.984,5.098,2.83l10.18-6.31L249.353,389.421z M381.071,315.061 c-0.87,1.683-2.854,3.878-5.366,5.415c-2.513,1.554-5.432,2.464-8.229,2.456c-0.993,0-1.952,0.17-2.887,0.414 c-1.708-0.374-3.416-0.894-5.098-1.561c-5.212-2.033-10.082-5.326-13.522-8.107c-1.724-1.39-3.09-2.642-4.001-3.512l-1.008-1 l-0.22-0.228l-0.033-0.032H340.7c-3.351-3.643-9.026-3.887-12.677-0.537c-3.651,3.351-3.903,9.026-0.544,12.701 c0.3,0.309,4.814,5.236,12.205,10.311c3.708,2.537,8.139,5.123,13.237,7.123c1.179,0.464,2.407,0.894,3.668,1.285 c0.35,0.788,0.797,1.537,1.325,2.244l-0.024,0.017c0.065,0.048,0.553,1.594,0.512,3.578c0.058,3.309-1.146,7.643-2.878,10.033 c-0.845,1.22-1.707,1.992-2.544,2.472c-0.854,0.472-1.716,0.781-3.334,0.805l-0.578-0.008c-0.342-0.016-0.666,0.041-1,0.057 c-0.016,0-0.024,0-0.024,0c-7.79,0.033-15.042-3.309-20.393-6.968c-2.667-1.805-4.806-3.643-6.245-4.992 c-0.708-0.667-1.252-1.22-1.586-1.561l-0.349-0.374l-0.057-0.073h-0.008c-3.244-3.724-8.895-4.131-12.644-0.886 c-3.757,3.252-4.163,8.92-0.911,12.676l-0.008-0.007c0.35,0.39,4.497,5.17,11.71,10.082c4.748,3.211,10.944,6.537,18.303,8.456 c-0.081,0.212-0.13,0.423-0.228,0.635c-0.943,2.13-2.293,4.325-5.968,6.472c-2.481,1.416-4.253,2.155-5.497,2.529 c-1.252,0.374-1.951,0.448-2.846,0.455c-1.162,0.017-2.911-0.244-5.822-0.838c-0.528-0.113-1.056-0.146-1.585-0.178 c-0.196-0.082-0.382-0.155-0.594-0.261c-2.976-1.399-6.603-3.928-9.278-6.082c-1.342-1.065-2.48-2.041-3.261-2.732l-0.878-0.797 l-0.203-0.195l-0.041-0.032c-3.586-3.431-9.278-3.301-12.709,0.284c-3.423,3.586-3.293,9.278,0.293,12.701 c0.187,0.17,3.968,3.798,9.269,7.603c0.943,0.667,1.984,1.342,3.025,2.008c-1.464,2.334-2.992,3.936-4.488,5.001 c-2.391,1.643-4.993,2.423-8.343,2.44c-3,0.008-6.53-0.732-10.058-2.228c-1.171-0.488-2.317-1.179-3.472-1.821 c0.675-2.521,1.049-5.131,1.041-7.75c0.008-5.391-1.472-10.904-4.513-15.791c-5.163-8.302-13.774-13.181-22.792-13.978 c0-0.326,0.056-0.65,0.056-0.984c0.008-5.911-1.618-11.977-4.952-17.344v0.017c-6.228-10.034-17.011-15.58-27.972-15.556 c-0.422,0-0.854,0.073-1.276,0.09c-0.553-4.66-2.082-9.278-4.699-13.49h0.007c-5.082-8.188-13.229-13.326-22.019-14.937 c-0.056-5.814-1.658-11.742-4.936-17.011h0.008c-6.22-10.009-17.011-15.563-27.972-15.547c-5.928,0-11.969,1.627-17.344,4.96 l-11.538,7.172c-0.277-0.634-0.626-1.252-0.878-1.887c-2.008-4.805-3.269-9.546-3.407-12.814l-0.268-5.058l-38.598-33.695 l73.18-75.792c3.741,3.066,7.579,6.107,11.515,8.953c6.521,4.7,12.985,8.847,19.588,11.628c4.237,1.748,8.367,2.179,12.157,2.187 c6.066-0.024,11.627-1.065,17.425-1.911c0.374-0.065,0.764-0.097,1.146-0.155c-7.968,6.838-14.921,14.238-20.8,21.126 c-4.952,5.814-9.131,11.294-12.441,15.766c-3.268,4.448-5.789,8.034-6.895,9.449c-5.448,7.09-7.789,15.571-7.838,24.068 c0.024,7.904,2.122,16.132,7.57,23.085c2.716,3.439,6.326,6.448,10.635,8.497c4.302,2.065,9.229,3.122,14.36,3.122 c6.367-0.017,13.042-1.561,20.109-4.53c22.182-9.383,45.047-15.636,62.252-19.49c8.603-1.919,15.799-3.252,20.816-4.09 c0.944-0.155,1.732-0.285,2.513-0.414l75.271,64.888l0.032,0.016l0.025,0.041C381.73,313.378,381.615,314.053,381.071,315.061z M389.78,260.395l-0.261,5.058c-0.138,2.894-1.122,6.976-2.773,11.205c-0.838,2.211-1.862,4.44-2.935,6.634l-65.92-56.821 c-2.561-2.211-5.976-3.236-9.334-2.813c-0.585,0.114-48.934,6.18-97.047,26.46c-4.789,2.024-8.383,2.65-10.79,2.634 c-1.959,0-3.155-0.35-4.041-0.773c-1.285-0.626-2.204-1.513-3.114-3.212c-0.87-1.674-1.464-4.146-1.455-6.748 c-0.049-3.74,1.285-7.546,2.78-9.35c1.863-2.407,4.139-5.692,7.278-9.937c4.658-6.31,10.985-14.474,18.53-22.418 c7.529-7.944,16.287-15.636,25.467-21.036c12.417-7.293,22.996-9.123,33.022-9.156c6.171-0.008,12.09,0.764,17.864,1.634 c5.789,0.854,11.367,1.878,17.417,1.911c3.781-0.008,7.887-0.439,12.108-2.163l0.056-0.016c6.603-2.781,13.067-6.928,19.588-11.628 c3.912-2.83,7.766-5.887,11.49-8.944l72.035,74.604L389.78,260.395z"></path></g></g></svg>';
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
