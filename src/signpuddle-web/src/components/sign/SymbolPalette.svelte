<script>
  import { onMount } from 'svelte';
  import { symbolStore } from '../../stores/symbolStore';
  
  export let onSelectSymbol;
  
  let searchQuery = '';
  let filteredSymbols = [];
  
  onMount(async () => {
    await symbolStore.loadCategories();
    if ($symbolStore.categories.length > 0) {
      selectCategory($symbolStore.categories[0].id);
    }
  });
  
  function selectCategory(categoryId) {
    symbolStore.loadSymbolsByCategory(categoryId);
  }
  
  function handleSymbolClick(symbol) {
    if (onSelectSymbol) {
      onSelectSymbol(symbol);
    }
  }
  
  // Filter symbols based on search query
  $: {
    if ($symbolStore.symbols) {
      filteredSymbols = searchQuery
        ? $symbolStore.symbols.filter(symbol => 
            symbol.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
            symbol.code.toLowerCase().includes(searchQuery.toLowerCase())
          )
        : $symbolStore.symbols;
    }
  }
</script>

<div class="symbol-palette">
  <div class="palette-header">
    <h3>Symbol Palette</h3>
    <div class="search-box">
      <input
        type="text"
        placeholder="Search symbols..."
        bind:value={searchQuery}
      />
    </div>
  </div>
  
  <div class="categories">
    {#if $symbolStore.isLoading && $symbolStore.categories.length === 0}
      <p>Loading categories...</p>
    {:else if $symbolStore.error}
      <p class="error">Error: {$symbolStore.error}</p>
    {:else}
      {#each $symbolStore.categories as category}
        <button 
          class:active={$symbolStore.selectedCategory === category.id}
          on:click={() => selectCategory(category.id)}
        >
          {category.name}
        </button>
      {/each}
    {/if}
  </div>
  
  <div class="symbols-container">
    {#if $symbolStore.isLoading && $symbolStore.symbols.length === 0}
      <p>Loading symbols...</p>
    {:else if $symbolStore.error}
      <p class="error">Error: {$symbolStore.error}</p>
    {:else}
      <div class="symbols-grid">
        {#each filteredSymbols as symbol}
          <div 
            class="symbol-item" 
            on:click={() => handleSymbolClick(symbol)}
            title={symbol.name}
          >
            <img src={symbol.imageSrc} alt={symbol.name} />
          </div>
        {/each}
      </div>
    {/if}
  </div>
</div>

<style>
  .symbol-palette {
    background-color: #f5f5f5;
    border-radius: 4px;
    padding: 1rem;
    display: flex;
    flex-direction: column;
    gap: 1rem;
    height: 100%;
    min-height: 300px;
  }
  
  .palette-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  h3 {
    margin: 0;
    font-size: 1rem;
    color: #444;
  }
  
  .search-box input {
    padding: 0.5rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    width: 150px;
  }
  
  .categories {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    border-bottom: 1px solid #ddd;
    padding-bottom: 1rem;
  }
  
  .categories button {
    padding: 0.4rem 0.8rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    background-color: #fff;
    cursor: pointer;
    font-size: 0.9rem;
  }
  
  .categories button.active {
    background-color: #1a3c6e;
    color: white;
    border-color: #1a3c6e;
  }
  
  .symbols-container {
    flex: 1;
    overflow-y: auto;
    min-height: 200px;
  }
  
  .symbols-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(50px, 1fr));
    gap: 0.5rem;
  }
  
  .symbol-item {
    background-color: white;
    border: 1px solid #ddd;
    border-radius: 4px;
    padding: 0.25rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    aspect-ratio: 1;
  }
  
  .symbol-item:hover {
    border-color: #1a3c6e;
    box-shadow: 0 0 5px rgba(26, 60, 110, 0.3);
  }
  
  .symbol-item img {
    max-width: 100%;
    max-height: 100%;
  }
  
  .error {
    color: red;
  }
</style>