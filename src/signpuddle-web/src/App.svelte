\SignWriting\SignPuddle2\src\signpuddle-web\src\App.svelte
<script>
  import { onMount } from 'svelte';
  import { Router, Route } from 'svelte-routing';
  import Header from './components/common/Header.svelte';
  import Footer from './components/common/Footer.svelte';
  import Home from './routes/Home.svelte';
  
  // Our routes - we'll add more as they're implemented
  import SignMaker from './routes/SignMaker.svelte';
  
  // Import userStore to check authentication on app load
  import { userStore } from './stores/userStore';
  
  export let url = '';
  
  onMount(() => {
    // Check if user is authenticated on app load
    const checkAuthStatus = async () => {
      try {
        // This would normally call userStore.checkAuth() which would validate the token with the backend
        // For now we'll rely on the initialization from localStorage in the store
      } catch (error) {
        console.error('Authentication check failed:', error);
      }
    };
    
    checkAuthStatus();
  });
</script>

<Router {url}>
  <div class="app">
    <Header />
    
    <main>
      <Route path="/" component={Home} />
      <Route path="/sign-maker" component={SignMaker} />
      <!-- Add more routes as components are implemented -->
      <!-- <Route path="/sign-text" component={SignText} /> -->
      <!-- <Route path="/dictionary/*" component={Dictionary} /> -->
      <!-- <Route path="/my-puddles" component={MyPuddles} /> -->
      <!-- <Route path="/settings" component={Settings} /> -->
    </main>
    
    <Footer />
  </div>
</Router>

<style>
  .app {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }
  
  main {
    flex: 1;
    padding-bottom: 2rem;
  }
  
  :global(body) {
    margin: 0;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen,
      Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
    line-height: 1.5;
    color: #333;
  }
  
  :global(*, *::before, *::after) {
    box-sizing: border-box;
  }
  
  :global(h1, h2, h3, h4, h5, h6) {
    color: #1a3c6e;
  }
  
  :global(a) {
    color: #1a3c6e;
  }
  
  :global(button) {
    font-family: inherit;
  }
</style>