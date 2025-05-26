import { writable } from 'svelte/store';
import { api } from '../services/api';

const createSymbolStore = () => {
  const initialState = {
    symbols: [],
    categories: [],
    selectedCategory: null,
    isLoading: false,
    error: null
  };

  const { subscribe, set, update } = writable(initialState);

  return {
    subscribe,
    
    // Load all symbol categories
    loadCategories: async () => {
      update(state => ({ ...state, isLoading: true }));
      try {
        const categories = await api.symbols.getCategories();
        update(state => ({
          ...state,
          categories,
          isLoading: false
        }));
      } catch (error) {
        update(state => ({
          ...state,
          error: error.message,
          isLoading: false
        }));
      }
    },
    
    // Load symbols for a specific category
    loadSymbolsByCategory: async (categoryId) => {
      update(state => ({ ...state, isLoading: true }));
      try {
        const symbols = await api.symbols.getByGroup(categoryId);
        update(state => ({
          ...state,
          symbols,
          selectedCategory: categoryId,
          isLoading: false
        }));
      } catch (error) {
        update(state => ({
          ...state,
          error: error.message,
          isLoading: false
        }));
      }
    },
    
    // Set the selected category without loading symbols
    selectCategory: (categoryId) => {
      update(state => ({ ...state, selectedCategory: categoryId }));
    },
    
    // Clear any error message
    clearError: () => {
      update(state => ({ ...state, error: null }));
    }
  };
};

export const symbolStore = createSymbolStore();