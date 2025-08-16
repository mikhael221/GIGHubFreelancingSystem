# Chat Integration Guide

## Overview
The chat system now supports manual chat initiation. Users can start chats by clicking on chat buttons/icons on other users' profiles.

## Components Available

### 1. Chat Button (`_ChatButton.cshtml`)
A full button with text and icon for prominent placement.

**Usage:**
```html
@await Html.PartialAsync("_ChatButton", user.Id)
```

**Example placement in user profile:**
```html
<div class="user-profile">
    <img src="@user.Photo" alt="@user.FirstName" class="w-20 h-20 rounded-full">
    <h2>@user.FirstName @user.LastName</h2>
    <p>@user.Email</p>
    
    <!-- Chat button -->
    @await Html.PartialAsync("_ChatButton", user.Id)
</div>
```

### 2. Chat Icon (`_ChatIcon.cshtml`)
A compact icon that can be overlaid on profile pictures.

**Usage:**
```html
<div class="relative">
    <img src="@user.Photo" alt="@user.FirstName" class="w-16 h-16 rounded-full">
    @await Html.PartialAsync("_ChatIcon", user.Id)
</div>
```

**Example placement with profile picture:**
```html
<div class="user-card">
    <div class="relative">
        <img src="@user.Photo" alt="@user.FirstName" class="w-16 h-16 rounded-full">
        @await Html.PartialAsync("_ChatIcon", user.Id)
    </div>
    <h3>@user.FirstName @user.LastName</h3>
    <p>@user.Role</p>
</div>
```

## How It Works

1. **User clicks chat button/icon** → Redirects to `/Chat/StartChat?targetUserId={userId}`
2. **System checks for existing chat** → If chat exists, redirects to that chat
3. **Creates new chat room** → If no chat exists, creates a new general chat room
4. **Redirects to chat** → User is taken to the chat interface

## Features

- ✅ **Prevents self-chatting** - Users can't start chats with themselves
- ✅ **Reuses existing chats** - If a chat already exists, it opens that chat
- ✅ **Automatic chat creation** - New chats are created automatically
- ✅ **Welcome message** - System message is added to new chats
- ✅ **Real-time messaging** - Full SignalR support for instant messaging

## Integration Examples

### In Project Details (for freelancers)
```html
<div class="project-client-info">
    <div class="relative">
        <img src="@project.User.Photo" alt="@project.User.FirstName" class="w-12 h-12 rounded-full">
        @await Html.PartialAsync("_ChatIcon", project.User.Id)
    </div>
    <span>@project.User.FirstName @project.User.LastName</span>
</div>
```

### In User List/Grid
```html
@foreach (var user in users)
{
    <div class="user-card">
        <div class="relative">
            <img src="@user.Photo" alt="@user.FirstName" class="w-16 h-16 rounded-full">
            @await Html.PartialAsync("_ChatIcon", user.Id)
        </div>
        <h3>@user.FirstName @user.LastName</h3>
        <p>@user.Role</p>
    </div>
}
```

### In User Profile Page
```html
<div class="profile-header">
    <img src="@user.Photo" alt="@user.FirstName" class="w-24 h-24 rounded-full">
    <div class="profile-info">
        <h1>@user.FirstName @user.LastName</h1>
        <p>@user.Email</p>
        <p>@user.Role</p>
        
        <!-- Chat button for other users -->
        @await Html.PartialAsync("_ChatButton", user.Id)
    </div>
</div>
```

## Styling Notes

- The chat button uses Tailwind CSS classes
- The chat icon is positioned absolutely and should be placed in a relative container
- Both components automatically hide themselves if the user is viewing their own profile
- Hover effects and transitions are included for better UX

## Security

- Users can only start chats with valid user IDs
- The system prevents users from chatting with themselves
- All chat access is controlled by authentication and authorization
