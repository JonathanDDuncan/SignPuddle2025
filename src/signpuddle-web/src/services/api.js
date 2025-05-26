// Simple approach with hardcoded fallback
const API_BASE_URL = (window.env && window.env.API_URL) || 'https://localhost:60317/api';

// Helper function to handle API responses
async function handleResponse(response) {
  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(
      errorData?.message || 
      `API error: ${response.status} ${response.statusText}`
    );
  }
  
  return response.json();
}

// Get auth headers for authenticated requests
function getAuthHeaders() {
  const token = localStorage.getItem('auth_token');
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export const api = {
  // Symbols API
  symbols: {
    // Get all symbol categories/groups
    getCategories: async () => {
      // For initial development, return mock data
      return [
        { id: "01", name: "Hands" },
        { id: "02", name: "Movement" },
        { id: "03", name: "Head/Face" },
        { id: "04", name: "Torso" },
        { id: "05", name: "Dynamics" }
      ];
      
      // When API is ready:
      // const response = await fetch(`${API_BASE_URL}/symbols/categories`);
      // return handleResponse(response);
    },
    
    // Get symbols by category/group
    getByGroup: async (groupId) => {
      // For initial development, return mock data
      const mockSymbols = [];
      
      // Generate 20 mock symbols
      for (let i = 1; i <= 20; i++) {
        mockSymbols.push({
          id: `${groupId}-${i}`,
          code: `S${groupId}${i.toString().padStart(2, '0')}`,
          name: `Symbol ${i}`,
          groupId: groupId,
          imageSrc: `https://via.placeholder.com/50?text=${groupId}-${i}`, // Placeholder image
          width: 50,
          height: 50
        });
      }
      
      return mockSymbols;
      
      // When API is ready:
      // const response = await fetch(`${API_BASE_URL}/symbols/group/${groupId}`);
      // return handleResponse(response);
    },
    
    // Get a specific symbol by ID
    getById: async (symbolId) => {
      const response = await fetch(`${API_BASE_URL}/symbols/${symbolId}`);
      return handleResponse(response);
    }
  },
  
  // Signs API - stubbed for now, implement when needed
  signs: {
    getById: async (id) => {
      const response = await fetch(`${API_BASE_URL}/signs/${id}`);
      return handleResponse(response);
    },
    
    create: async (signData) => {
      const response = await fetch(`${API_BASE_URL}/signs`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...getAuthHeaders()
        },
        body: JSON.stringify(signData)
      });
      return handleResponse(response);
    },
    
    update: async (id, signData) => {
      const response = await fetch(`${API_BASE_URL}/signs/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          ...getAuthHeaders()
        },
        body: JSON.stringify(signData)
      });
      return handleResponse(response);
    }
  }
};