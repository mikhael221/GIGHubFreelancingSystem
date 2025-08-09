// Global Video Call Notification System
class GlobalVideoCall {
    constructor() {
        this.connection = null;
        this.currentUserId = null;
        this.isConnected = false;
        this.notificationContainer = null;
        this.init();
    }

    async init() {
        try {
            // Get current user ID from page
            this.currentUserId = this.getCurrentUserId();
            if (!this.currentUserId) {
                return;
            }

            // Create notification container
            this.createNotificationContainer();
            
            // Connect to SignalR
            await this.connectToSignalR();
            
            // Set up event handlers
            this.setupEventHandlers();
            
        } catch (error) {
            console.error('GlobalVideoCall: Error initializing:', error);
        }
    }

    getCurrentUserId() {
        // Try to get user ID from body data attribute
        const body = document.querySelector('body[data-user-id]');
        if (body && body.dataset.userId) {
            return body.dataset.userId;
        }

        // Try to get user ID from various sources
        const userIdElement = document.querySelector('[data-user-id]');
        if (userIdElement && userIdElement.dataset.userId) {
            return userIdElement.dataset.userId;
        }

        // Check if we're on a chat page
        const matchIdElement = document.querySelector('[data-match-id]');
        if (matchIdElement) {
            // Extract from URL or other sources
            const urlParams = new URLSearchParams(window.location.search);
            const userId = urlParams.get('userId') || this.extractUserIdFromPage();
            if (userId) {
                return userId;
            }
        }

        return null;
    }

    extractUserIdFromPage() {
        // Try to extract from various page elements
        const scripts = document.querySelectorAll('script');
        for (const script of scripts) {
            const text = script.textContent;
            const match = text.match(/currentUserId['"]?\s*[:=]\s*['"]([^'"]+)['"]/);
            if (match) {
                return match[1];
            }
        }
        return null;
    }

    createNotificationContainer() {
        // Remove existing container if any
        const existing = document.getElementById('global-video-notification');
        if (existing) {
            existing.remove();
        }

        // Create new container
        this.notificationContainer = document.createElement('div');
        this.notificationContainer.id = 'global-video-notification';
        this.notificationContainer.style.cssText = `
            position: fixed !important;
            top: 20px !important;
            right: 20px !important;
            z-index: 99999 !important;
            max-width: 400px !important;
            min-width: 300px !important;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif !important;
            pointer-events: auto !important;
            display: block !important;
            visibility: visible !important;
        `;

        document.body.appendChild(this.notificationContainer);
    }

    async connectToSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/mentorshipChatHub')
                .withAutomaticReconnect()
                .build();

            await this.connection.start();
            this.isConnected = true;

            // Join global room for this user
            await this.connection.invoke('JoinUserRoom', this.currentUserId);

        } catch (error) {
            console.error('GlobalVideoCall: Failed to connect to SignalR:', error);
        }
    }

    setupEventHandlers() {
        if (!this.connection) {
            return;
        }

        // Handle incoming video calls
        this.connection.on('IncomingVideoCall', (data) => {
            this.showIncomingCallNotification(data);
        });

        // Handle call waiting events
        this.connection.on('CallRequested', (data) => {
            this.showCallWaitingNotification(data);
        });

        this.connection.on('CallAccepted', (data) => {
            this.hideCallWaitingNotification();
        });

        this.connection.on('CallDeclined', (data) => {
            this.hideCallWaitingNotification();
        });

        // Handle video call ended
        this.connection.on('VideoCallEnded', () => {
            this.hideNotification();
        });

        // Handle connection state changes
        this.connection.onclose(() => {
            this.isConnected = false;
        });

        this.connection.onreconnecting(() => {
            // Reconnecting...
        });

        this.connection.onreconnected(() => {
            this.isConnected = true;
            // Rejoin user room
            this.connection.invoke('JoinUserRoom', this.currentUserId);
        });

        // Handle errors
        this.connection.on('Error', (error) => {
            console.error('GlobalVideoCall: SignalR error:', error);
        });
    }

    showIncomingCallNotification(data) {
        const notification = document.createElement('div');
        notification.className = 'video-call-notification';
        
        // Create a beautiful notification
        notification.style.cssText = `
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            color: white !important;
            padding: 20px !important;
            border-radius: 12px !important;
            box-shadow: 0 10px 25px rgba(0,0,0,0.2) !important;
            margin-bottom: 10px !important;
            animation: slideIn 0.3s ease-out !important;
            backdrop-filter: blur(10px) !important;
            border: 1px solid rgba(255,255,255,0.1) !important;
            position: relative !important;
            z-index: 10000 !important;
            min-width: 300px !important;
            max-width: 400px !important;
            display: block !important;
        `;

        // Force display block directly on the element
        notification.style.display = 'block';
        notification.style.visibility = 'visible';
        notification.style.opacity = '1';

        notification.innerHTML = `
            <div style="display: flex; align-items: center; gap: 15px;">
                <div style="flex-shrink: 0;">
                    <div style="width: 50px; height: 50px; background: rgba(255,255,255,0.2); border-radius: 50%; display: flex; align-items: center; justify-content: center;">
                        <i class="fas fa-video" style="font-size: 20px;"></i>
                    </div>
                </div>
                <div style="flex-grow: 1;">
                    <h4 style="margin: 0 0 5px 0; font-size: 16px; font-weight: 600;">
                        Incoming Video Call
                    </h4>
                    <p style="margin: 0; font-size: 14px; opacity: 0.9;">
                        ${data.callerName || 'Unknown'} is calling...
                    </p>
                </div>
                <div style="flex-shrink: 0; display: flex; gap: 8px;">
                    <button onclick="window.globalVideoCall.acceptCall('${data.mentorshipMatchId}', '${data.callerId}')" 
                            style="background: #10b981; border: none; color: white; padding: 8px 12px; border-radius: 6px; cursor: pointer; font-size: 12px;">
                        <i class="fas fa-phone"></i> Accept
                    </button>
                    <button onclick="window.globalVideoCall.declineCall('${data.mentorshipMatchId}', '${data.callerId}')" 
                            style="background: #ef4444; border: none; color: white; padding: 8px 12px; border-radius: 6px; cursor: pointer; font-size: 12px;">
                        <i class="fas fa-phone-slash"></i> Decline
                    </button>
                </div>
            </div>
        `;

        // Add CSS animation
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
            @keyframes slideOut {
                from { transform: translateX(0); opacity: 1; }
                to { transform: translateX(100%); opacity: 0; }
            }
        `;
        document.head.appendChild(style);

        if (this.notificationContainer) {
            this.notificationContainer.appendChild(notification);
        } else {
            console.error('GlobalVideoCall: Notification container is null!');
        }

        // Auto-hide after 30 seconds
        setTimeout(() => {
            this.hideNotification();
        }, 30000);
    }

    hideNotification() {
        if (this.notificationContainer) {
            const notifications = this.notificationContainer.querySelectorAll('.video-call-notification');
            notifications.forEach(notification => {
                notification.style.animation = 'slideOut 0.3s ease-in';
                setTimeout(() => {
                    if (notification.parentNode) {
                        notification.remove();
                    }
                }, 300);
            });
        }
    }

    showCallWaitingNotification(data) {
        const notification = document.createElement('div');
        notification.className = 'call-waiting-notification';
        
        // Create a beautiful waiting notification
        notification.style.cssText = `
            background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%) !important;
            color: white !important;
            padding: 20px !important;
            border-radius: 12px !important;
            box-shadow: 0 10px 25px rgba(0,0,0,0.2) !important;
            margin-bottom: 10px !important;
            animation: slideIn 0.3s ease-out !important;
            backdrop-filter: blur(10px) !important;
            border: 1px solid rgba(255,255,255,0.1) !important;
            position: relative !important;
            z-index: 10000 !important;
            min-width: 300px !important;
            max-width: 400px !important;
            display: block !important;
        `;

        // Force display block directly on the element
        notification.style.display = 'block';
        notification.style.visibility = 'visible';
        notification.style.opacity = '1';

        notification.innerHTML = `
            <div style="display: flex; align-items: center; gap: 15px;">
                <div style="flex-shrink: 0;">
                    <div style="width: 50px; height: 50px; background: rgba(255,255,255,0.2); border-radius: 50%; display: flex; align-items: center; justify-content: center;">
                        <i class="fas fa-phone" style="font-size: 20px;"></i>
                    </div>
                </div>
                <div style="flex-grow: 1;">
                    <h4 style="margin: 0 0 5px 0; font-size: 16px; font-weight: 600;">
                        Calling...
                    </h4>
                    <p style="margin: 0; font-size: 14px; opacity: 0.9;">
                        Waiting for response
                    </p>
                </div>
                <div style="flex-shrink: 0;">
                    <button onclick="window.globalVideoCall.cancelCall()" 
                            style="background: #ef4444; border: none; color: white; padding: 8px 12px; border-radius: 6px; cursor: pointer; font-size: 12px;">
                        <i class="fas fa-times"></i> Cancel
                    </button>
                </div>
            </div>
        `;

        if (this.notificationContainer) {
            this.notificationContainer.appendChild(notification);
        } else {
            console.error('GlobalVideoCall: Notification container is null!');
        }
    }

    hideCallWaitingNotification() {
        if (this.notificationContainer) {
            const notifications = this.notificationContainer.querySelectorAll('.call-waiting-notification');
            notifications.forEach(notification => {
                notification.style.animation = 'slideOut 0.3s ease-in';
                setTimeout(() => {
                    if (notification.parentNode) {
                        notification.remove();
                    }
                }, 300);
            });
        }
    }

    cancelCall() {
        // Hide the waiting notification
        this.hideCallWaitingNotification();
        
        // Note: We don't need to send a specific cancel event to the server
        // The waiting notification will just disappear, and if the other person
        // tries to accept/decline later, it won't affect anything
    }

    async acceptCall(matchId, callerId) {
        try {
            // Hide notification
            this.hideNotification();
            
            // Send accept signal
            if (this.connection && this.isConnected) {
                await this.connection.invoke('AcceptVideoCall', matchId, callerId);
            }
            
            // Open video call window
            const videoCallUrl = `/MentorshipChat/VideoCall/${matchId}`;
            window.open(videoCallUrl, 'videoCall', 'width=1200,height=800,scrollbars=no,resizable=yes');
            
        } catch (error) {
            console.error('GlobalVideoCall: Error accepting call:', error);
            alert('Failed to accept video call. Please try again.');
        }
    }

    async declineCall(matchId, callerId) {
        try {
            // Hide notification
            this.hideNotification();
            
            // Send decline signal
            if (this.connection && this.isConnected) {
                await this.connection.invoke('DeclineVideoCall', matchId, callerId);
            }
            
        } catch (error) {
            console.error('GlobalVideoCall: Error declining call:', error);
        }
    }

    // Public method to check if connected
    isSignalRConnected() {
        return this.isConnected;
    }
}

// Initialize global video call system when page loads
let globalVideoCall = null;

document.addEventListener('DOMContentLoaded', () => {
    // Only initialize if SignalR is available
    if (typeof signalR !== 'undefined') {
        globalVideoCall = new GlobalVideoCall();
        
        // Make it globally accessible
        window.globalVideoCall = globalVideoCall;
    }
});

// Also make it globally accessible immediately
window.globalVideoCall = globalVideoCall; 