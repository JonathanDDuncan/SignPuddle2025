import App from './App.svelte';
import { Router } from 'svelte-routing'; // Add this import

// Replace line causing error (line 5) with proper usage
const app = new App({
  target: document.body,
  props: {
    // If needed, pass Router as a prop or use it in your component hierarchy
  }
});

export default app;