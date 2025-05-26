<script>
  import SignEditor from '../components/sign/SignEditor.svelte';
  import { userStore } from '../stores/userStore';
  import { onMount } from 'svelte';
  
  let initialFSW = '';
  let signId = null;
  let signName = '';
  let signDefinition = '';
  
  // Check for URL parameters to load an existing sign
  onMount(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const id = urlParams.get('id');
    
    if (id) {
      loadSignById(id);
    }
  });
  
  async function loadSignById(id) {
    try {
      // In a real app, this would fetch from your API
      // const sign = await api.signs.getById(id);
      
      // For now, use mock data
      const sign = {
        id: id,
        fsw: "M525x535S2ff00482x483S10000490x490S2e300500x515S10100508x523",
        name: "Example Sign",
        definition: "This is a sample sign definition"
      };
      
      initialFSW = sign.fsw;
      signId = sign.id;
      signName = sign.name;
      signDefinition = sign.definition;
    } catch (error) {
      console.error("Error loading sign:", error);
      // Show error message
    }
  }
  
  async function handleSave() {
    if (!$userStore.isAuthenticated) {
      userStore.showLoginModal();
      return;
    }
    
    // In a real app, this would save to your API
    // For now, just log the values
    console.log("Save sign:", {
      id: signId,
      name: signName,
      definition: signDefinition
      // The FSW would come from the sign store
    });
    
    // Show success message
    alert("Sign saved successfully!");
  }
</script>

<div class="sign-maker-page">
  <header class="page-header">
    <h1>Sign Maker</h1>
    <p>Create and edit individual signs using SignWriting notation</p>
  </header>
  
  <div class="content-container">
    <div class="editor-container">
      <SignEditor editorMode="edit" initialSignFSW={initialFSW} />
    </div>
    
    <div class="metadata-panel">
      <h2>Sign Information</h2>
      
      <div class="form-group">
        <label for="sign-name">Name</label>
        <input 
          type="text" 
          id="sign-name" 
          bind:value={signName} 
          placeholder="Enter sign name"
        />
      </div>
      
      <div class="form-group">
        <label for="sign-definition">Definition</label>
        <textarea 
          id="sign-definition" 
          bind:value={signDefinition} 
          placeholder="Enter sign definition"
          rows="5"
        ></textarea>
      </div>
      
      <button class="save-button" on:click={handleSave}>
        {signId ? 'Update Sign' : 'Save Sign'}
      </button>
      
      {#if !$userStore.isAuthenticated}
        <div class="login-notice">
          <p>You need to <button class="link-button" on:click={() => userStore.showLoginModal()}>log in</button> to save signs.</p>
        </div>
      {/if}
    </div>
  </div>
</div>

<style>
  .sign-maker-page {
    max-width: 1200px;
    margin: 0 auto;
    padding: 1rem;
  }
  
  .page-header {
    text-align: center;
    margin-bottom: 2rem;
  }
  
  .page-header h1 {
    margin-bottom: 0.5rem;
  }
  
  .page-header p {
    color: #666;
  }
  
  .content-container {
    display: flex;
    flex-direction: column;
    gap: 2rem;
  }
  
  @media (min-width: 900px) {
    .content-container {
      flex-direction: row;
    }
    
    .editor-container {
      flex: 2;
    }
    
    .metadata-panel {
      flex: 1;
    }
  }
  
  .editor-container {
    min-height: 500px;
  }
  
  .metadata-panel {
    background-color: #f5f8ff;
    padding: 1.5rem;
    border-radius: 8px;
  }
  
  .form-group {
    margin-bottom: 1rem;
  }
  
  label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 500;
  }
  
  input, textarea {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    font-family: inherit;
    font-size: 1rem;
  }
  
  .save-button {
    background-color: #1a3c6e;
    color: white;
    border: none;
    border-radius: 4px;
    padding: 0.75rem 1.5rem;
    font-size: 1rem;
    cursor: pointer;
    width: 100%;
    margin-top: 1rem;
  }
  
  .save-button:hover {
    background-color: #2c5aa0;
  }
  
  .login-notice {
    margin-top: 1rem;
    padding: 0.75rem;
    background-color: #e9ecef;
    border-radius: 4px;
    text-align: center;
  }
  
  .link-button {
    background: none;
    border: none;
    color: #1a3c6e;
    padding: 0;
    font-size: inherit;
    text-decoration: underline;
    cursor: pointer;
  }
</style>