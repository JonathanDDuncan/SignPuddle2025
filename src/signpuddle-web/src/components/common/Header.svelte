<script>
  import { userStore } from '../../stores/userStore';
  import LoginModal from '../auth/LoginModal.svelte';
  
  function handleLogin() {
    userStore.showLoginModal();
  }
  
  function handleLogout() {
    userStore.logout();
  }
</script>

<header>
  <div class="logo">
    <a href="/">
      <h1>SignPuddle 2</h1>
    </a>
  </div>
  
  <nav>
    <ul>
      <li><a href="/sign-maker">Sign Maker</a></li>
      <li><a href="/sign-text">Sign Text</a></li>
      <li><a href="/dictionary">Dictionary</a></li>
    </ul>
  </nav>
  
  <div class="user-controls">
    {#if $userStore.isAuthenticated}
      <span class="username">Welcome, {$userStore.username}</span>
      <div class="dropdown">
        <button class="dropdown-toggle">My Account</button>
        <div class="dropdown-menu">
          <a href="/my-puddles">My Puddles</a>
          <a href="/settings">Settings</a>
          <button on:click={handleLogout}>Logout</button>
        </div>
      </div>
    {:else}
      <button class="login-button" on:click={handleLogin}>Login</button>
    {/if}
  </div>
</header>

<LoginModal />

<style>
  header {
    background-color: #1a3c6e;
    color: white;
    padding: 1rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .logo h1 {
    margin: 0;
    font-size: 1.5rem;
  }
  
  .logo a {
    color: white;
    text-decoration: none;
  }
  
  nav ul {
    display: flex;
    list-style: none;
    margin: 0;
    padding: 0;
  }
  
  nav li {
    margin: 0 1rem;
  }
  
  nav a {
    color: white;
    text-decoration: none;
    padding: 0.5rem;
  }
  
  nav a:hover {
    text-decoration: underline;
  }
  
  .user-controls {
    display: flex;
    align-items: center;
    gap: 1rem;
  }
  
  .username {
    display: none;
    
    @media (min-width: 768px) {
      display: inline;
    }
  }
  
  button {
    padding: 0.5rem 1rem;
    cursor: pointer;
    background-color: #2c5aa0;
    color: white;
    border: none;
    border-radius: 4px;
  }
  
  button:hover {
    background-color: #3a6cb5;
  }
  
  .login-button {
    background-color: transparent;
    border: 1px solid white;
  }
  
  .login-button:hover {
    background-color: rgba(255, 255, 255, 0.1);
  }
  
  /* Dropdown styling */
  .dropdown {
    position: relative;
    display: inline-block;
  }
  
  .dropdown-toggle {
    background-color: #2c5aa0;
    cursor: pointer;
  }
  
  .dropdown-menu {
    display: none;
    position: absolute;
    top: 100%;
    right: 0;
    z-index: 1000;
    min-width: 180px;
    padding: 0.5rem 0;
    margin: 0.125rem 0 0;
    background-color: white;
    border-radius: 4px;
    box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
  }
  
  .dropdown-menu a, 
  .dropdown-menu button {
    display: block;
    width: 100%;
    text-align: left;
    padding: 0.5rem 1rem;
    background: none;
    border: none;
    color: #333;
    text-decoration: none;
  }
  
  .dropdown-menu a:hover,
  .dropdown-menu button:hover {
    background-color: #f5f5f5;
    color: #1a3c6e;
  }
  
  .dropdown:hover .dropdown-menu {
    display: block;
  }
</style>