<script>
  import { onMount } from 'svelte';
  import { signStore } from '../../stores/signStore';
  import { symbolStore } from '../../stores/symbolStore';
  import SymbolPalette from './SymbolPalette.svelte';
  import SignControls from './SignControls.svelte';
  
  export let editorMode = 'edit';  // 'edit' or 'view'
  export let initialSignFSW = '';
  
  let canvas;
  let canvasContext;
  let isDragging = false;
  let selectedSymbolId = null;
  let startX, startY;
  let signContainer;
  
  onMount(() => {
    canvas = document.getElementById('sign-canvas');
    canvasContext = canvas.getContext('2d');
    
    if (initialSignFSW) {
      loadSign(initialSignFSW);
    }
    
    setCanvasDimensions();
    renderCanvas();
  });
  
  function setCanvasDimensions() {
    if (!signContainer) return;
    
    const rect = signContainer.getBoundingClientRect();
    canvas.width = rect.width;
    canvas.height = rect.height;
  }
  
  function loadSign(fsw) {
    // In a real implementation, this would parse the FSW format
    // and create the symbol objects
    // For now, we'll just set it directly in the store
    signStore.setSign({
      fsw: fsw,
      symbols: []  // This would be populated from FSW parsing
    });
  }
  
  function renderCanvas() {
    if (!canvas) return;
    
    canvasContext.clearRect(0, 0, canvas.width, canvas.height);
    
    const sign = $signStore.currentSign;
    if (!sign || !sign.symbols) return;
    
    // Draw each symbol
    sign.symbols.forEach(symbol => {
      drawSymbol(symbol);
      
      // Highlight selected symbol
      if (selectedSymbolId === symbol.id) {
        drawSelectionBox(symbol);
      }
    });
  }
  
  function drawSymbol(symbol) {
    // In a real implementation, this would use the symbol data
    // to render the actual symbol. This is just a placeholder.
    const img = new Image();
    img.src = symbol.imageSrc;
    
    canvasContext.drawImage(
      img, 
      symbol.x, 
      symbol.y, 
      symbol.width, 
      symbol.height
    );
  }
  
  function drawSelectionBox(symbol) {
    canvasContext.strokeStyle = 'blue';
    canvasContext.lineWidth = 2;
    canvasContext.strokeRect(
      symbol.x - 2, 
      symbol.y - 2, 
      symbol.width + 4, 
      symbol.height + 4
    );
  }
  
  function handleCanvasMouseDown(event) {
    if (editorMode !== 'edit') return;
    
    const rect = canvas.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;
    
    // Check if we clicked on a symbol
    const sign = $signStore.currentSign;
    if (!sign || !sign.symbols) return;
    
    for (let i = sign.symbols.length - 1; i >= 0; i--) {
      const symbol = sign.symbols[i];
      if (x >= symbol.x && x <= symbol.x + symbol.width &&
          y >= symbol.y && y <= symbol.y + symbol.height) {
        selectedSymbolId = symbol.id;
        isDragging = true;
        startX = x;
        startY = y;
        renderCanvas();
        return;
      }
    }
    
    // Clicked outside any symbol
    selectedSymbolId = null;
    renderCanvas();
  }
  
  function handleCanvasMouseMove(event) {
    if (!isDragging || !selectedSymbolId || editorMode !== 'edit') return;
    
    const rect = canvas.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;
    
    const deltaX = x - startX;
    const deltaY = y - startY;
    
    // Update the symbol position
    signStore.updateSymbol(selectedSymbolId, {
      x: symbol => symbol.x + deltaX,
      y: symbol => symbol.y + deltaY
    });
    
    startX = x;
    startY = y;
    renderCanvas();
  }
  
  function handleCanvasMouseUp() {
    isDragging = false;
  }
  
  function handleAddSymbol(symbol) {
    const newSymbol = {
      ...symbol,
      id: generateUniqueId(),
      x: canvas.width / 2 - symbol.width / 2,
      y: canvas.height / 2 - symbol.height / 2
    };
    
    signStore.addSymbol(newSymbol);
    renderCanvas();
  }
  
  function generateUniqueId() {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }
  
  // Watch for store changes to update canvas
  $: $signStore && renderCanvas();
</script>

<div class="sign-editor">
  <div class="sign-canvas-container" bind:this={signContainer}>
    <canvas 
      id="sign-canvas"
      on:mousedown={handleCanvasMouseDown}
      on:mousemove={handleCanvasMouseMove}
      on:mouseup={handleCanvasMouseUp}
      on:mouseleave={handleCanvasMouseUp}
    ></canvas>
  </div>
  
  {#if editorMode === 'edit'}
    <div class="editor-controls">
      <SignControls 
        onClear={() => signStore.clearSign()}
        onUndo={() => signStore.undo()}
        selectedSymbolId={selectedSymbolId} 
      />
      
      <SymbolPalette onSelectSymbol={handleAddSymbol} />
    </div>
  {/if}
</div>

<style>
  .sign-editor {
    display: flex;
    flex-direction: column;
    height: 100%;
  }
  
  .sign-canvas-container {
    flex: 1;
    border: 1px solid #ccc;
    background-color: white;
    margin-bottom: 1rem;
    position: relative;
    min-height: 400px;
  }
  
  canvas {
    position: absolute;
    top: 0;
    left: 0;
  }
  
  .editor-controls {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }
</style>