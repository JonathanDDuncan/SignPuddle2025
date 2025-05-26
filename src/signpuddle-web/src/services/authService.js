const API_BASE_URL = process.env.API_URL || 'http://localhost:5000/api';

export const authService = {
  login: async (username, password) => {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ username, password })
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to login');
    }
    
    const data = await response.json();
    localStorage.setItem('auth_token', data.token);
    
    return {
      id: data.id,
      username: data.username,
      email: data.email
    };
  },
  
  register: async (userData) => {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(userData)
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to register');
    }
    
    return response.json();
  },
  
  logout: () => {
    localStorage.removeItem('auth_token');
    return Promise.resolve();
  },
  
  checkAuth: async () => {
    const token = localStorage.getItem('auth_token');
    if (!token) return Promise.resolve(null);
    
    const response = await fetch(`${API_BASE_URL}/auth/validate`, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    
    if (!response.ok) {
      localStorage.removeItem('auth_token');
      return null;
    }
    
    return response.json();
  }
};