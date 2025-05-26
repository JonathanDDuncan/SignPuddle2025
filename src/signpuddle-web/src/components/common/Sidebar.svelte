<script>
  import { Link } from 'svelte-routing';
  import { userStore } from '../../stores/userStore';
</script>

<aside class="sidebar">
  <div class="sidebar-section">
    <h3>Tools</h3>
    <ul>
      <li><Link to="/sign-maker">Sign Maker</Link></li>
      <li><Link to="/sign-text">Sign Text</Link></li>
    </ul>
  </div>

  <div class="sidebar-section">
    <h3>Dictionaries</h3>
    <ul>
      <li><Link to="/dictionary">Browse</Link></li>
      <li><Link to="/dictionary/search">Search</Link></li>
    </ul>
  </div>

  {#if $userStore.isAuthenticated}
    <div class="sidebar-section">
      <h3>My Content</h3>
      <ul>
        <li><Link to="/my-puddles">My Puddles</Link></li>
        <li><Link to="/settings">Settings</Link></li>
      </ul>
    </div>
  {/if}
</aside>

<style>
  .sidebar {
    width: 200px;
    background-color: #f5f5f5;
    padding: 1rem;
  }
  
  .sidebar-section {
    margin-bottom: 2rem;
  }
  
  h3 {
    margin-top: 0;
    border-bottom: 1px solid #ddd;
    padding-bottom: 0.5rem;
  }
  
  ul {
    list-style: none;
    padding: 0;
    margin: 0;
  }
  
  li {
    margin-bottom: 0.5rem;
  }
  
  a {
    text-decoration: none;
    color: #1a3c6e;
  }
</style>