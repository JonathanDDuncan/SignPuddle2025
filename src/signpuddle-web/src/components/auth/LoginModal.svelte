<script>
  import { userStore } from '../../stores/userStore';
  
  let username = '';
  let password = '';
  let isRegistering = false;
  let email = '';
  let confirmPassword = '';
  
  function toggleForm() {
    isRegistering = !isRegistering;
    clearForm();
  }
  
  function clearForm() {
    username = '';
    password = '';
    email = '';
    confirmPassword = '';
    $userStore.clearError();
  }
  
  async function handleSubmit() {
    if (isRegistering) {
      // Registration form validation
      if (!username || !password || !email || !confirmPassword) {
        return;
      }
      
      if (password !== confirmPassword) {
        userStore.update(state => ({ ...state, error: "Passwords don't match" }));
        return;
      }
      
      try {
        await userStore.register({ username, password, email });
        // Auto-login after successful registration
        await userStore.login(username, password);
      } catch (error) {
        // Error is handled in the store
      }
    } else {
      // Login form validation
      if (!username || !password) {
        return;
      }
      
      try {
        await userStore.login(username, password);
      } catch (error) {
        // Error is handled in the store
      }
    }
  }
</script>

{#if $userStore.loginModalOpen}
  <div class="modal-backdrop" on:click={() => userStore.hideLoginModal()}>
    <div class="modal-content" on:click|stopPropagation>
      <button class="close-button" on:click={() => userStore.hideLoginModal()}>×</button>
      
      <h2>{isRegistering ? 'Create Account' : 'Log In'}</h2>
      
      {#if $userStore.error}
        <div class="error-message">
          {$userStore.error}
        </div>
      {/if}
      
      <form on:submit|preventDefault={handleSubmit}>
        <div class="form-group">
          <label for="username">Username</label>
          <input 
            type="text" 
            id="username" 
            bind:value={username} 
            placeholder="Username"
            required
          />
        </div>
        
        {#if isRegistering}
          <div class="form-group">
            <label for="email">Email</label>
            <input 
              type="email" 
              id="email" 
              bind:value={email} 
              placeholder="Email address"
              required
            />
          </div>
        {/if}
        
        <div class="form-group">
          <label for="password">Password</label>
          <input 
            type="password" 
            id="password" 
            bind:value={password} 
            placeholder="Password"
            required
          />
        </div>
        
        {#if isRegistering}
          <div class="form-group">
            <label for="confirm-password">Confirm Password</label>
            <input 
              type="password" 
              id="confirm-password" 
              bind:value={confirmPassword} 
              placeholder="Confirm password"
              required
            />
          </div>
        {/if}
        
        <button 
          type="submit" 
          class="primary-button"
          disabled={$userStore.loading}
        >
          {#if $userStore.loading}
            Loading...
          {:else}
            {isRegistering ? 'Register' : 'Log In'}
          {/if}
        </button>
      </form>
      
      <div class="form-footer">
        {#if isRegistering}
          Already have an account? 
          <button class="link-button" on:click={toggleForm}>Log In</button>
        {:else}
          Don't have an account? 
          <button class="link-button" on:click={toggleForm}>Register</button>
        {/if}
      </div>
    </div>
  </div>
{/if}

<style>
  .modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.7);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }
  
  .modal-content {
    background-color: white;
    border-radius: 8px;
    padding: 2rem;
    width: 100%;
    max-width: 400px;
    position: relative;
  }
  
  .close-button {
    position: absolute;
    top: 1rem;
    right: 1rem;
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
    color: #666;
  }
  
  h2 {
    margin-top: 0;
    margin-bottom: 1.5rem;
    color: #1a3c6e;
  }
  
  .form-group {
    margin-bottom: 1.2rem;
  }
  
  label {
    display: block;
    margin-bottom: 0.5rem;
    color: #333;
  }
  
  input {
    width: 100%;
    padding: 0.8rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    font-size: 1rem;
  }
  
  .primary-button {
    width: 100%;
    padding: 0.8rem;
    background-color: #1a3c6e;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 1rem;
    margin-top: 1rem;
  }
  
  .primary-button:hover {
    background-color: #2c5aa0;
  }
  
  .primary-button:disabled {
    background-color: #6c8cbf;
    cursor: not-allowed;
  }
  
  .form-footer {
    margin-top: 1.5rem;
    text-align: center;
    color: #666;
  }
  
  .link-button {
    background: none;
    border: none;
    color: #1a3c6e;
    cursor: pointer;
    padding: 0;
    font-size: inherit;
    text-decoration: underline;
  }
  
  .error-message {
    background-color: #f8d7da;
    color: #721c24;
    padding: 0.75rem;
    border-radius: 4px;
    margin-bottom: 1rem;
    border: 1px solid #f5c6cb;
  }
</style>