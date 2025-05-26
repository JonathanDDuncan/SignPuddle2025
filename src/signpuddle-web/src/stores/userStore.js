import { writable } from 'svelte/store';

const createUserStore = () => {
  // Initialize from localStorage if available
  const storedUser = localStorage.getItem('user');
  const storedToken = localStorage.getItem('auth_token');
  
  const initialState = {
    isAuthenticated: !!storedToken,
    username: storedUser ? JSON.parse(storedUser).username : null,
    userId: storedUser ? JSON.parse(storedUser).id : null,
    email: storedUser ? JSON.parse(storedUser).email : null,
    loginModalOpen: false,
    error: null,
    loading: false
  };

  const { subscribe, set, update } = writable(initialState);

  return {
    subscribe,
    
    login: async (username, password) => {
      update(state => ({ ...state, loading: true, error: null }));
      
      try {
        // In a real app, this would call the API
        // For now, simulate a successful login with mock data
        await new Promise(resolve => setTimeout(resolve, 800));
        
        // Simulate success for demo purposes (mock data)
        const userData = {
          id: 'user123',
          username: username,
          email: `${username}@example.com`
        };
        
        const mockToken = 'mock-auth-token-' + Math.random().toString(36).slice(2);
        
        // Store in localStorage
        localStorage.setItem('auth_token', mockToken);
        localStorage.setItem('user', JSON.stringify(userData));
        
        update(state => ({
          ...state,
          isAuthenticated: true,
          username: userData.username,
          userId: userData.id,
          email: userData.email,
          loginModalOpen: false,
          loading: false,
          error: null
        }));
        
        return userData;
        
        // Real implementation would be:
        // const response = await fetch('/api/auth/login', {
        //   method: 'POST',
        //   headers: { 'Content-Type': 'application/json' },
        //   body: JSON.stringify({ username, password })
        // });
        // const data = await response.json();
        // if (!response.ok) throw new Error(data.message || 'Login failed');
        // localStorage.setItem('auth_token', data.token);
        // localStorage.setItem('user', JSON.stringify(data.user));
        // update(...)
      } catch (error) {
        update(state => ({ 
          ...state, 
          error: error.message || 'Login failed', 
          loading: false 
        }));
        throw error;
      }
    },
    
    register: async (userData) => {
      update(state => ({ ...state, loading: true, error: null }));
      
      try {
        // In a real app, this would call the API
        // For now, simulate a successful registration with mock data
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        // Return success for demo purposes
        update(state => ({
          ...state,
          loading: false,
        }));
        
        return { success: true };
      } catch (error) {
        update(state => ({ 
          ...state, 
          error: error.message || 'Registration failed', 
          loading: false 
        }));
        throw error;
      }
    },
    
    logout: () => {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('user');
      
      set({
        isAuthenticated: false,
        username: null,
        userId: null,
        email: null,
        loginModalOpen: false,
        error: null,
        loading: false
      });
    },
    
    showLoginModal: () => update(state => ({ ...state, loginModalOpen: true })),
    
    hideLoginModal: () => update(state => ({ ...state, loginModalOpen: false })),
    
    clearError: () => update(state => ({ ...state, error: null }))
  };
};

export const userStore = createUserStore();