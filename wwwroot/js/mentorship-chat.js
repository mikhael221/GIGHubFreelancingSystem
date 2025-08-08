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
    }

    // Send message (server handles encryption)
    async sendMessage(message, messageType = 'text') {
        try {
            await this.connection.invoke('SendMessage', this.matchId, message, messageType);
        } catch (error) {
            console.error('Failed to send message:', error);
            alert('Failed to send message');
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

// Event listeners
document.addEventListener('DOMContentLoaded', function () {
    const messageInput = document.getElementById('messageInput');

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

    // Mark messages as read when page becomes visible
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden && mentorshipChat) {
            mentorshipChat.markMessagesAsRead();
        }
    });
});