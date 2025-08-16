// Simple chat client for server-side encryption
class MentorshipChat {
    constructor(connection, matchId, currentUserId) {
        this.connection = connection;
        this.matchId = matchId;
        this.currentUserId = currentUserId;
        this.setupEventHandlers();
    }

    setupEventHandlers() {
        // Handle receiving messages (server handles decryption)
        this.connection.on('ReceiveMessage', (message) => {
            this.addMessageToChat(message);
        });

        // Handle receiving files
        this.connection.on('ReceiveFile', (fileMessage) => {
            this.addFileMessageToChat(fileMessage);
        });

        // Handle connection events
        this.connection.on('Connected', (connectionId) => {
            console.log('Connected to chat:', connectionId);
        });

        this.connection.on('JoinedRoom', (roomName) => {
            console.log('Joined room:', roomName);
        });

        this.connection.on('Error', (error) => {
            console.error('Chat error:', error);
            alert('Chat error: ' + error);
        });

        // Handle typing indicators
        this.connection.on('TypingIndicator', (data) => {
            this.handleTypingIndicator(data);
        });

        // Handle video call events
        this.connection.on('IncomingVideoCall', (data) => {
            this.handleIncomingVideoCall(data);
        });

        this.connection.on('VideoCallAccepted', (data) => {
            this.handleVideoCallAccepted(data);
        });

        this.connection.on('VideoCallDeclined', (data) => {
            this.handleVideoCallDeclined(data);
        });

        this.connection.on('VideoCallEnded', (data) => {
            this.handleVideoCallEnded(data);
        });
    }

    // Send message (server handles encryption)
    async sendMessage(message, messageType = 'text') {
        try {
            await this.connection.invoke('SendMessage', this.matchId, message, messageType, null);
        } catch (error) {
            console.error('Failed to send message:', error);
            alert('Failed to send message');
        }
    }

    // Send file message
    async sendFile(fileName, fileUrl, fileSize, fileType) {
        try {
            await this.connection.invoke('SendFile', this.matchId, fileName, fileUrl, fileSize, fileType);
        } catch (error) {
            console.error('Failed to send file:', error);
            alert('Failed to send file');
        }
    }

    // Join mentorship room
    async joinRoom() {
        try {
            await this.connection.invoke('JoinMentorshipRoom', this.matchId);
        } catch (error) {
            console.error('Failed to join room:', error);
        }
    }

    // Add message to chat UI
    addMessageToChat(message) {
        console.log('Processing message:', message);

        const messagesContainer = document.getElementById('messagesContainer');
        const messageDiv = document.createElement('div');

        const senderId = message.SenderId || message.senderId || '';
        const senderName = message.SenderName || message.senderName || 'Unknown User';
        const messageText = message.Message || message.message || '';
        const sentAt = message.SentAt || message.sentAt || new Date().toISOString();

        const isOwnMessage = senderId.toString() === this.currentUserId.toString();
        messageDiv.className = `message ${isOwnMessage ? 'own' : ''}`;

        let timeDisplay = 'Now';
        try {
            const messageDate = new Date(sentAt);
            if (!isNaN(messageDate.getTime())) {
                timeDisplay = messageDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            }
        } catch (e) {
            console.error('Date parsing error:', e);
        }

        messageDiv.innerHTML = `
            <div class="message-bubble">
                ${!isOwnMessage ? `<div class="message-sender">${senderName}</div>` : ''}
                <div class="message-content">${messageText}</div>
                <div class="message-time">
                    ${timeDisplay}
                    ${isOwnMessage ? '<i class="fas fa-check" title="Sent"></i>' : ''}
                    <i class="fas fa-shield-alt text-green-500" title="Server Encrypted" style="margin-left: 5px;"></i>
                </div>
            </div>
        `;

        messagesContainer.appendChild(messageDiv);
        this.scrollToBottom();
    }

    // Add file message to chat UI
    addFileMessageToChat(fileMessage) {
        console.log('Processing file message:', fileMessage);

        const messagesContainer = document.getElementById('messagesContainer');
        const messageDiv = document.createElement('div');

        const senderId = fileMessage.SenderId || fileMessage.senderId || '';
        const senderName = fileMessage.SenderName || fileMessage.senderName || 'Unknown User';
        const fileName = fileMessage.FileName || fileMessage.fileName || '';
        const fileUrl = fileMessage.FileUrl || fileMessage.fileUrl || '';
        const fileSize = fileMessage.FileSize || fileMessage.fileSize || 0;
        const fileType = fileMessage.FileType || fileMessage.fileType || '';
        const sentAt = fileMessage.SentAt || fileMessage.sentAt || new Date().toISOString();

        const isOwnMessage = senderId.toString() === this.currentUserId.toString();
        messageDiv.className = `message ${isOwnMessage ? 'own' : ''}`;

        let timeDisplay = 'Now';
        try {
            const messageDate = new Date(sentAt);
            if (!isNaN(messageDate.getTime())) {
                timeDisplay = messageDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            }
        } catch (e) {
            console.error('Date parsing error:', e);
        }

        // Format file size
        const formatFileSize = (bytes) => {
            if (bytes === 0) return '0 Bytes';
            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
        };

        // Get file icon based on type
        const getFileIcon = (type) => {
            if (type.startsWith('image/')) return 'fas fa-image';
            if (type.startsWith('video/')) return 'fas fa-video';
            if (type.startsWith('audio/')) return 'fas fa-music';
            if (type.includes('pdf')) return 'fas fa-file-pdf';
            if (type.includes('word') || type.includes('document')) return 'fas fa-file-word';
            return 'fas fa-file';
        };

        messageDiv.innerHTML = `
            <div class="message-bubble">
                ${!isOwnMessage ? `<div class="message-sender">${senderName}</div>` : ''}
                <div class="message-content">
                    <div class="file-message">
                        <i class="${getFileIcon(fileType)} text-blue-500 mr-2"></i>
                        <a href="${fileUrl}" target="_blank" class="file-link">
                            ${fileName}
                        </a>
                        <span class="file-size text-gray-500 text-sm">(${formatFileSize(fileSize)})</span>
                    </div>
                </div>
                <div class="message-time">
                    ${timeDisplay}
                    ${isOwnMessage ? '<i class="fas fa-check" title="Sent"></i>' : ''}
                    <i class="fas fa-shield-alt text-green-500" title="Server Encrypted" style="margin-left: 5px;"></i>
                </div>
            </div>
        `;

        messagesContainer.appendChild(messageDiv);
        this.scrollToBottom();
    }

    // Mark messages as read
    async markMessagesAsRead() {
        try {
            await this.connection.invoke('MarkMessagesAsRead', this.matchId);
        } catch (error) {
            console.error('Failed to mark messages as read:', error);
        }
    }

    // Send typing indicator
    async sendTypingIndicator(isTyping) {
        try {
            await this.connection.invoke('SendTypingIndicator', this.matchId, isTyping);
        } catch (error) {
            console.error('Failed to send typing indicator:', error);
        }
    }

    // Handle typing indicators from other users
    handleTypingIndicator(data) {
        const typingDiv = document.getElementById('typingIndicator');
        if (data.UserId !== this.currentUserId) {
            if (data.IsTyping) {
                typingDiv.textContent = `${data.UserName} is typing...`;
                typingDiv.style.display = 'block';
            } else {
                typingDiv.style.display = 'none';
            }
        }
    }

    // Handle incoming video call
    handleIncomingVideoCall(data) {
        console.log('Incoming video call:', data);
        window.currentCallerId = data.CallerId;
        document.getElementById('videoCallNotification').style.display = 'block';
        
        // Auto-hide notification after 30 seconds
        setTimeout(() => {
            document.getElementById('videoCallNotification').style.display = 'none';
        }, 30000);
    }

    // Handle video call accepted
    handleVideoCallAccepted(data) {
        console.log('Video call accepted:', data);
        // The video call window should already be open
    }

    // Handle video call declined
    handleVideoCallDeclined(data) {
        console.log('Video call declined:', data);
        // Could show a notification that the call was declined
    }

    // Handle video call ended
    handleVideoCallEnded(data) {
        console.log('Video call ended:', data);
        // Could show a notification that the call ended
    }

    scrollToBottom() {
        const container = document.getElementById('messagesContainer');
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }
}

// Global chat instance
let mentorshipChat;

// Initialize chat when SignalR connection is established
connection.start().then(() => {
    console.log('SignalR Connected');

    // Initialize chat
    mentorshipChat = new MentorshipChat(connection, matchId, currentUserId);

    // Join the room
    mentorshipChat.joinRoom();

}).catch(err => console.error('SignalR Connection Error: ', err));

// Send message function
function sendMessage() {
    const input = document.getElementById('messageInput');
    const message = input.value.trim();

    if (message && mentorshipChat) {
        mentorshipChat.sendMessage(message).then(() => {
            input.value = '';
            handleTyping(false); // Stop typing indicator
        });
    }
}

// Handle typing indicators
let typingTimer;
function handleTyping(isTyping = true) {
    if (!mentorshipChat) return;

    if (isTyping) {
        mentorshipChat.sendTypingIndicator(true);

        // Clear existing timer
        clearTimeout(typingTimer);

        // Set timer to stop typing indicator
        typingTimer = setTimeout(() => {
            mentorshipChat.sendTypingIndicator(false);
        }, 2000);
    } else {
        clearTimeout(typingTimer);
        mentorshipChat.sendTypingIndicator(false);
    }
}

// Video call functions
function startVideoCall() {
    if (connection && connection.state === 'Connected') {
        connection.invoke('StartVideoCall', matchId);
        // Open video call window immediately for the caller
        window.open(`/MentorshipChat/VideoCall/${matchId}`, '_blank', 'width=1200,height=800');
    } else {
        alert('Connection not established. Please wait and try again.');
    }
}

function acceptVideoCall() {
    if (connection && connection.state === 'Connected' && window.currentCallerId) {
        connection.invoke('AcceptVideoCall', matchId, window.currentCallerId);
        document.getElementById('videoCallNotification').style.display = 'none';
        window.open(`/MentorshipChat/VideoCall/${matchId}`, '_blank', 'width=1200,height=800');
    }
}

function declineVideoCall() {
    if (connection && connection.state === 'Connected' && window.currentCallerId) {
        connection.invoke('DeclineVideoCall', matchId, window.currentCallerId);
        document.getElementById('videoCallNotification').style.display = 'none';
    }
}

// File upload function
function uploadFile() {
    const fileInput = document.getElementById('fileInput');
    const file = fileInput.files[0];
    
    if (!file) {
        alert('Please select a file to upload.');
        return;
    }

    // Check file size (10MB limit)
    if (file.size > 10 * 1024 * 1024) {
        alert('File size must be less than 10MB.');
        return;
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('matchId', matchId);

    fetch(`/MentorshipChat/UploadFile/${matchId}`, {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Send file message through SignalR
            mentorshipChat.sendFile(data.fileName, data.fileUrl, data.fileSize, data.fileType);
            fileInput.value = ''; // Clear the input
        } else {
            alert('Upload failed: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Upload error:', error);
        alert('Upload failed. Please try again.');
    });
}

// Event listeners
document.addEventListener('DOMContentLoaded', function () {
    const messageInput = document.getElementById('messageInput');
    const fileInput = document.getElementById('fileInput');

    if (messageInput) {
        messageInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            } else {
                handleTyping(true);
            }
        });

        messageInput.addEventListener('input', function () {
            if (this.value.trim()) {
                handleTyping(true);
            }
        });
    }

    if (fileInput) {
        fileInput.addEventListener('change', function () {
            if (this.files.length > 0) {
                uploadFile();
            }
        });
    }

    // Mark messages as read when page becomes visible
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden && mentorshipChat) {
            mentorshipChat.markMessagesAsRead();
        }
    });
});