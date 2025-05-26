import { writable } from 'svelte/store';

const createSignStore = () => {
  const initialState = {
    currentSign: {
      fsw: '',
      symbols: []
    },
    isModified: false,
    history: []
  };

  const { subscribe, set, update } = writable(initialState);

  return {
    subscribe,
    setSign: (sign) => {
      update(state => ({
        ...state,
        currentSign: sign,
        isModified: false,
        history: [state.currentSign, ...state.history.slice(0, 9)]
      }));
    },
    addSymbol: (symbol) => {
      update(state => {
        const newSymbols = [...state.currentSign.symbols, symbol];
        return {
          ...state,
          currentSign: {
            ...state.currentSign,
            symbols: newSymbols
          },
          isModified: true
        };
      });
    },
    removeSymbol: (symbolId) => {
      update(state => {
        const newSymbols = state.currentSign.symbols.filter(
          sym => sym.id !== symbolId
        );
        return {
          ...state,
          currentSign: {
            ...state.currentSign,
            symbols: newSymbols
          },
          isModified: true
        };
      });
    },
    updateSymbol: (symbolId, updates) => {
      update(state => {
        const newSymbols = state.currentSign.symbols.map(sym => 
          sym.id === symbolId ? { ...sym, ...updates } : sym
        );
        return {
          ...state,
          currentSign: {
            ...state.currentSign,
            symbols: newSymbols
          },
          isModified: true
        };
      });
    },
    clearSign: () => {
      update(state => ({
        ...state,
        currentSign: {
          fsw: '',
          symbols: []
        },
        isModified: false,
        history: [state.currentSign, ...state.history.slice(0, 9)]
      }));
    },
    undo: () => {
      update(state => {
        if (state.history.length === 0) return state;
        
        const [previousSign, ...restHistory] = state.history;
        return {
          ...state,
          currentSign: previousSign,
          history: restHistory,
          isModified: true
        };
      });
    }
  };
};

export const signStore = createSignStore();