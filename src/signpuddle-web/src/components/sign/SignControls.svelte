<script>
  import { signStore } from '../../stores/signStore';
  
  export let onClear;
  export let onUndo;
  export let selectedSymbolId = null;
  
  function handleDelete() {
    if (selectedSymbolId) {
      signStore.removeSymbol(selectedSymbolId);
    }
  }
  
  function handleRotate(direction) {
    if (selectedSymbolId) {
      const degrees = direction === 'clockwise' ? 45 : -45;
      signStore.updateSymbol(selectedSymbolId, {
        rotation: symbol => (symbol.rotation || 0) + degrees
      });
    }
  }
  
  function handleFlip(axis) {
    if (selectedSymbolId) {
      signStore.updateSymbol(selectedSymbolId, {
        [axis === 'horizontal' ? 'flipX' : 'flipY']: symbol => !(symbol[axis === 'horizontal' ? 'flipX' : 'flipY'])
      });
    }
  }
</script>

<div class="sign-controls">
  <div class="control-group">
    <h3>Edit</h3>
    <button on:click={onClear} title="Clear Sign">
      <span class="icon">🗑️</span> Clear
    </button>
    <button on:click={onUndo} title="Undo">
      <span class="icon">↩️</span> Undo
    </button>
  </div>
  
  <div class="control-group">
    <h3>Symbol</h3>
    <button 
      on:click={handleDelete} 
      disabled={!selectedSymbolId} 
      title="Delete Selected Symbol"
    >
      <span class="icon">❌</span> Delete
    </button>
    <button 
      on:click={() => handleRotate('clockwise')} 
      disabled={!selectedSymbolId} 
      title="Rotate Clockwise"
    >
      <span class="icon">🔄</span> Rotate ⟳
    </button>
    <button 
      on:click={() => handleRotate('counterclockwise')} 
      disabled={!selectedSymbolId} 
      title="Rotate Counter-clockwise"
    >
      <span class="icon">🔄</span> Rotate ⟲
    </button>
    <button 
      on:click={() => handleFlip('horizontal')} 
      disabled={!selectedSymbolId} 
      title="Flip Horizontally"
    >
      <span class="icon">⇄</span> Flip ↔
    </button>
    <button 
      on:click={() => handleFlip('vertical')} 
      disabled={!selectedSymbolId} 
      title="Flip Vertically"
    >
      <span class="icon">⇅</span> Flip ↕
    </button>
  </div>

  <div class="control-group">
    <h3>Save</h3>
    <button title="Save Sign">
      <span class="icon">💾</span> Save
    </button>
  </div>
</div>

<style>
  .sign-controls {
    display: flex;
    flex-wrap: wrap;
    gap: 1.5rem;
    padding: 1rem;
    background-color: #f5f5f5;
    border-radius: 4px;
  }
  
  .control-group {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }
  
  h3 {
    margin: 0 0 0.5rem 0;
    font-size: 1rem;
    color: #444;
  }
  
  button {
    padding: 0.5rem;
    background-color: #fff;
    border: 1px solid #ccc;
    border-radius: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    font-size: 0.9rem;
  }
  
  button:hover {
    background-color: #f0f0f0;
  }
  
  button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .icon {
    font-size: 1.2rem;
  }
</style>