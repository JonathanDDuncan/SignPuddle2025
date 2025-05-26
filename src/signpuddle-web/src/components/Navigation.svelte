<script>
  import { Link } from 'svelte-routing';
  import { userStore } from '../stores/userStore';
  
  function logout() {
    userStore.logout();
    window.location.href = '/';
  }
</script>

<nav>
  <div class="container nav-container">
    <div class="logo">
      <Link to="/">SignPuddle 2</Link>
    </div>
    
    <div class="menu">
      <Link to="/sign-editor">Sign Editor</Link>
      <Link to="/sign-text-editor">Sign Text Editor</Link>
      <Link to="/dictionaries">Dictionaries</Link>
      
      {#if $userStore.isLoggedIn}
        <div class="user-menu">
          <Link to="/profile">{$userStore.username}</Link>
          <button class="logout" on:click={logout}>Logout</button>
        </div>
      {:else}
        <Link to="/login">Login</Link>
        <Link to="/register">Register</Link>
      {/if}
    </div>
  </div>
</nav>

<style>
  nav {
    background-color: var(--primary-color);
    color: white;
    padding: 1em 0;
  }
  
  .nav-container {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .logo {
    font-size: 1.5em;
    font-weight: bold;
  }
  
  .logo a {
    color: white;
    text-decoration: none;
  }
  
  .menu {
    display: flex;
    gap: 1em;<!-- filepath: c:\Code\SignWriting\SignPuddle2\signpuddle-web\src\components\Navigation.svelte -->
<script>
  import { Link } from 'svelte-routing';
  import { userStore } from '../stores/userStore';
  
  function logout() {
    userStore.logout();
    window.location.href = '/';
  }
</script>

<nav>
  <div class="container nav-container">
    <div class="logo">
      <Link to="/">SignPuddle 2</Link>
    </div>
    
    <div class="menu">
      <Link to="/sign-editor">Sign Editor</Link>
      <Link to="/sign-text-editor">Sign Text Editor</Link>
      <Link to="/dictionaries">Dictionaries</Link>
      
      {#if $userStore.isLoggedIn}
        <div class="user-menu">
          <Link to="/profile">{$userStore.username}</Link>
          <button class="logout" on:click={logout}>Logout</button>
        </div>
      {:else}
        <Link to="/login">Login</Link>
        <Link to="/register">Register</Link>
      {/if}
    </div>
  </div>
</nav>

<style>
  nav {
    background-color: var(--primary-color);
    color: white;
    padding: 1em 0;
  }
  
  .nav-container {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .logo {
    font-size: 1.5em;
    font-weight: bold;
  }
  
  .logo a {
    color: white;
    text-decoration: none;
  }
  
  .menu {
    display: flex;
    gap: 1em;