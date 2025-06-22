// cypress/e2e/sign-editor.cy.js
describe('Sign Editor', () => {
    beforeEach(() => {
        cy.visit('/sign-maker')
        cy.intercept('GET', '/api/symbols/groups', { fixture: 'symbolGroups.json' }).as('getSymbolGroups')
        cy.intercept('GET', '/api/symbols/**', { fixture: 'symbols.json' }).as('getSymbols')
    })

    it('should load symbol palette on page load', () => {
        cy.wait('@getSymbolGroups')
        cy.get('[data-testid="symbol-palette"]').should('be.visible')
        cy.get('[data-testid="symbol-group"]').should('have.length.greaterThan', 0)
    })

    it('should create a new sign by selecting symbols', () => {
        cy.wait('@getSymbolGroups')
        
        // Select first symbol group
        cy.get('[data-testid="symbol-group"]').first().click()
        cy.wait('@getSymbols')
        
        // Select a symbol from palette
        cy.get('[data-testid="symbol-item"]').first().click()
        
        // Verify symbol appears in sign canvas
        cy.get('[data-testid="sign-canvas"]').should('contain.html', 'svg')
        cy.get('[data-testid="selected-symbols"]').should('have.length', 1)
    })

    it('should manipulate symbols (rotate, flip, resize)', () => {
        // Add a symbol first
        cy.get('[data-testid="symbol-group"]').first().click()
        cy.get('[data-testid="symbol-item"]').first().click()
        
        // Select the symbol in canvas
        cy.get('[data-testid="sign-canvas"] svg').first().click()
        
        // Test rotation
        cy.get('[data-testid="rotate-button"]').click()
        cy.get('[data-testid="sign-canvas"] svg').should('have.attr', 'transform').and('include', 'rotate')
        
        // Test flip
        cy.get('[data-testid="flip-horizontal-button"]').click()
        cy.get('[data-testid="sign-canvas"] svg').should('have.attr', 'transform').and('include', 'scale(-1,1)')
        
        // Test resize
        cy.get('[data-testid="resize-handle"]').trigger('mousedown', { which: 1 })
        cy.get('[data-testid="resize-handle"]').trigger('mousemove', { clientX: 100, clientY: 100 })
        cy.get('[data-testid="resize-handle"]').trigger('mouseup')
    })

    it('should save sign to dictionary', () => {
        cy.intercept('POST', '/api/signs', { statusCode: 201, body: { id: 123 } }).as('saveSign')
        
        // Create a sign
        cy.get('[data-testid="symbol-group"]').first().click()
        cy.get('[data-testid="symbol-item"]').first().click()
        
        // Open save dialog
        cy.get('[data-testid="save-button"]').click()
        cy.get('[data-testid="save-modal"]').should('be.visible')
        
        // Fill in sign details
        cy.get('[data-testid="sign-term-input"]').type('Test Sign')
        cy.get('[data-testid="sign-definition-input"]').type('A test sign definition')
        cy.get('[data-testid="dictionary-select"]').select('ASL Dictionary')
        
        // Save the sign
        cy.get('[data-testid="confirm-save-button"]').click()
        cy.wait('@saveSign')
        
        // Verify success message
        cy.get('[data-testid="success-message"]').should('contain', 'Sign saved successfully')
    })

    it('should handle undo/redo operations', () => {
        // Add multiple symbols
        cy.get('[data-testid="symbol-group"]').first().click()
        cy.get('[data-testid="symbol-item"]').first().click()
        cy.get('[data-testid="symbol-item"]').eq(1).click()
        
        cy.get('[data-testid="selected-symbols"]').should('have.length', 2)
        
        // Test undo
        cy.get('[data-testid="undo-button"]').click()
        cy.get('[data-testid="selected-symbols"]').should('have.length', 1)
        
        // Test redo
        cy.get('[data-testid="redo-button"]').click()
        cy.get('[data-testid="selected-symbols"]').should('have.length', 2)
    })
})

// cypress/e2e/sign-text-editor.cy.js
describe('Sign Text Editor', () => {
    beforeEach(() => {
        cy.visit('/sign-text-editor')
        cy.intercept('GET', '/api/signs/search**', { fixture: 'searchResults.json' }).as('searchSigns')
    })

    it('should load existing sign text sequence', () => {
        cy.intercept('GET', '/api/sign-texts/123', { fixture: 'signText.json' }).as('getSignText')
        cy.visit('/sign-text-editor/123')
        
        cy.wait('@getSignText')
        cy.get('[data-testid="sign-sequence"]').should('be.visible')
        cy.get('[data-testid="sign-item"]').should('have.length', 3)
    })

    it('should add signs to sequence', () => {
        // Search for signs
        cy.get('[data-testid="sign-search-input"]').type('hello')
        cy.wait('@searchSigns')
        
        // Add sign from search results
        cy.get('[data-testid="search-result"]').first().click()
        cy.get('[data-testid="add-to-sequence-button"]').click()
        
        // Verify sign added to sequence
        cy.get('[data-testid="sign-sequence"] [data-testid="sign-item"]').should('have.length', 1)
    })

    it('should reorder signs in sequence', () => {
        // Add multiple signs first
        cy.get('[data-testid="sign-search-input"]').type('hello')
        cy.get('[data-testid="search-result"]').first().click()
        cy.get('[data-testid="add-to-sequence-button"]').click()
        
        cy.get('[data-testid="sign-search-input"]').clear().type('world')
        cy.get('[data-testid="search-result"]').first().click()
        cy.get('[data-testid="add-to-sequence-button"]').click()
        
        // Drag and drop to reorder
        cy.get('[data-testid="sign-item"]').first().trigger('dragstart')
        cy.get('[data-testid="sign-item"]').last().trigger('drop')
        
        // Verify order changed
        cy.get('[data-testid="sign-item"]').first().should('contain', 'world')
    })

    it('should delete signs from sequence', () => {
        // Add a sign first
        cy.get('[data-testid="sign-search-input"]').type('hello')
        cy.get('[data-testid="search-result"]').first().click()
        cy.get('[data-testid="add-to-sequence-button"]').click()
        
        // Delete the sign
        cy.get('[data-testid="sign-item"] [data-testid="delete-sign-button"]').click()
        cy.get('[data-testid="confirm-delete-button"]').click()
        
        // Verify sign removed
        cy.get('[data-testid="sign-sequence"] [data-testid="sign-item"]').should('have.length', 0)
    })

    it('should save sign text sequence', () => {
        cy.intercept('POST', '/api/sign-texts', { statusCode: 201, body: { id: 456 } }).as('saveSignText')
        
        // Add signs to sequence
        cy.get('[data-testid="sign-search-input"]').type('hello')
        cy.get('[data-testid="search-result"]').first().click()
        cy.get('[data-testid="add-to-sequence-button"]').click()
        
        // Save sequence
        cy.get('[data-testid="save-sequence-button"]').click()
        cy.get('[data-testid="sequence-title-input"]').type('Test Sequence')
        cy.get('[data-testid="confirm-save-sequence-button"]').click()
        
        cy.wait('@saveSignText')
        cy.get('[data-testid="success-message"]').should('contain', 'Sequence saved successfully')
    })
})

// cypress/e2e/dictionary-browser.cy.js
describe('Dictionary Browser', () => {
    beforeEach(() => {
        cy.visit('/dictionary')
        cy.intercept('GET', '/api/dictionaries', { fixture: 'dictionaries.json' }).as('getDictionaries')
        cy.intercept('GET', '/api/dictionaries/*/signs**', { fixture: 'dictionarySigns.json' }).as('getDictionarySigns')
    })

    it('should display available dictionaries', () => {
        cy.wait('@getDictionaries')
        cy.get('[data-testid="dictionary-list"]').should('be.visible')
        cy.get('[data-testid="dictionary-item"]').should('have.length.greaterThan', 0)
    })

    it('should browse signs in selected dictionary', () => {
        cy.wait('@getDictionaries')
        
        // Select a dictionary
        cy.get('[data-testid="dictionary-item"]').first().click()
        cy.wait('@getDictionarySigns')
        
        // Verify signs are displayed
        cy.get('[data-testid="sign-grid"]').should('be.visible')
        cy.get('[data-testid="sign-card"]').should('have.length.greaterThan', 0)
    })

    it('should search signs in dictionary', () => {
        cy.intercept('GET', '/api/dictionaries/*/signs?search=*', { fixture: 'searchResults.json' }).as('searchDictionary')
        
        cy.get('[data-testid="dictionary-item"]').first().click()
        
        // Search for signs
        cy.get('[data-testid="dictionary-search-input"]').type('hello')
        cy.wait('@searchDictionary')
        
        // Verify search results
        cy.get('[data-testid="sign-card"]').should('contain', 'hello')
    })

    it('should view sign details', () => {
        cy.intercept('GET', '/api/signs/123', { fixture: 'signDetails.json' }).as('getSignDetails')
        
        cy.get('[data-testid="dictionary-item"]').first().click()
        cy.get('[data-testid="sign-card"]').first().click()
        
        cy.wait('@getSignDetails')
        cy.get('[data-testid="sign-details-modal"]').should('be.visible')
        cy.get('[data-testid="sign-term"]').should('contain', 'Test Sign')
        cy.get('[data-testid="sign-definition"]').should('contain', 'A test sign')
    })

    it('should filter signs by category', () => {
        cy.get('[data-testid="dictionary-item"]').first().click()
        
        // Apply category filter
        cy.get('[data-testid="category-filter"]').select('Animals')
        cy.get('[data-testid="apply-filter-button"]').click()
        
        // Verify filtered results
        cy.get('[data-testid="sign-card"]').each(($card) => {
            cy.wrap($card).should('have.attr', 'data-category', 'Animals')
        })
    })
})

// cypress/e2e/navigation.cy.js
describe('Navigation', () => {
    it('should navigate between main sections', () => {
        cy.visit('/')
        
        // Test navigation to Sign Maker
        cy.get('[data-testid="nav-sign-maker"]').click()
        cy.url().should('include', '/sign-maker')
        cy.get('[data-testid="sign-editor"]').should('be.visible')
        
        // Test navigation to Sign Text Editor
        cy.get('[data-testid="nav-sign-text"]').click()
        cy.url().should('include', '/sign-text-editor')
        cy.get('[data-testid="sign-text-editor"]').should('be.visible')
        
        // Test navigation to Dictionary
        cy.get('[data-testid="nav-dictionary"]').click()
        cy.url().should('include', '/dictionary')
        cy.get('[data-testid="dictionary-browser"]').should('be.visible')
        
        // Test navigation back to home
        cy.get('[data-testid="nav-home"]').click()
        cy.url().should('eq', Cypress.config().baseUrl + '/')
        cy.get('[data-testid="home-page"]').should('be.visible')
    })

    it('should handle invalid routes', () => {
        cy.visit('/invalid-route')
        cy.get('[data-testid="not-found-page"]').should('be.visible')
        cy.get('[data-testid="home-link"]').click()
        cy.url().should('eq', Cypress.config().baseUrl + '/')
    })
})

// cypress/e2e/api-integration.cy.js
describe('API Integration', () => {
    it('should handle API errors gracefully', () => {
        cy.intercept('GET', '/api/symbols/groups', { statusCode: 500 }).as('getSymbolGroupsError')
        cy.visit('/sign-maker')
        
        cy.wait('@getSymbolGroupsError')
        cy.get('[data-testid="error-message"]').should('contain', 'Failed to load symbol groups')
        cy.get('[data-testid="retry-button"]').should('be.visible')
    })

    it('should retry failed API calls', () => {
        cy.intercept('GET', '/api/symbols/groups', { statusCode: 500 }).as('getSymbolGroupsError')
        cy.intercept('GET', '/api/symbols/groups', { fixture: 'symbolGroups.json' }).as('getSymbolGroupsSuccess')
        
        cy.visit('/sign-maker')
        cy.wait('@getSymbolGroupsError')
        
        cy.get('[data-testid="retry-button"]').click()
        cy.wait('@getSymbolGroupsSuccess')
        cy.get('[data-testid="symbol-palette"]').should('be.visible')
    })

    it('should handle network timeouts', () => {
        cy.intercept('GET', '/api/symbols/groups', { delay: 30000 }).as('getSymbolGroupsTimeout')
        cy.visit('/sign-maker')
        
        cy.get('[data-testid="loading-spinner"]').should('be.visible')
        // Test timeout handling after reasonable wait
    })
})

// cypress/e2e/accessibility.cy.js
describe('Accessibility', () => {
    beforeEach(() => {
        cy.injectAxe()
    })

    it('should have no accessibility violations on home page', () => {
        cy.visit('/')
        cy.checkA11y()
    })

    it('should have no accessibility violations on sign maker', () => {
        cy.visit('/sign-maker')
        cy.checkA11y()
    })

    it('should support keyboard navigation', () => {
        cy.visit('/sign-maker')
        
        // Test tab navigation through symbol palette
        cy.get('body').tab()
        cy.focused().should('have.attr', 'data-testid', 'symbol-group')
        
        // Test enter key to select symbol
        cy.focused().type('{enter}')
        cy.get('[data-testid="symbol-item"]').first().should('be.focused')
    })

    it('should provide proper ARIA labels', () => {
        cy.visit('/sign-maker')
        
        cy.get('[data-testid="symbol-palette"]').should('have.attr', 'aria-label', 'Symbol palette')
        cy.get('[data-testid="sign-canvas"]').should('have.attr', 'aria-label', 'Sign editing canvas')
        cy.get('[data-testid="save-button"]').should('have.attr', 'aria-label', 'Save current sign')
    })
})

// cypress/support/commands.js
Cypress.Commands.add('tab', { prevSubject: 'element' }, (subject) => {
    cy.wrap(subject).trigger('keydown', { keyCode: 9 })
})

// cypress/fixtures/symbolGroups.json
[
    {
        "id": 1,
        "name": "Hand Shapes",
        "description": "Basic hand configurations",
        "symbolCount": 45
    },
    {
        "id": 2,
        "name": "Movements",
        "description": "Movement patterns",
        "symbolCount": 38
    }
]

// cypress/fixtures/symbols.json
[
    {
        "id": "S10000",
        "group": 1,
        "name": "Index finger",
        "svg": "<svg>...</svg>",
        "unicode": "𝠀"
    },
    {
        "id": "S10001",
        "group": 1,
        "name": "Thumb",
        "svg": "<svg>...</svg>",
        "unicode": "𝠁"
    }
]

// cypress/fixtures/signText.json
{
    "id": 123,
    "title": "Sample Sign Text",
    "signs": [
        {
            "id": 1,
            "term": "Hello",
            "fsw": "M525x535S2e748483x510S10011501x466S2e704510x500S10019476x475"
        },
        {
            "id": 2,
            "term": "World",
            "fsw": "M518x529S11541497x471S11549476x496S20600491x516"
        }
    ]
}

// cypress.config.js
import { defineConfig } from 'cypress'

export default defineConfig({
    e2e: {
        baseUrl: 'http://localhost:5000',
        supportFile: 'cypress/support/e2e.js',
        specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}',
        viewportWidth: 1280,
        viewportHeight: 720,
        video: true,
        screenshotOnRunFailure: true,
        setupNodeEvents(on, config) {
            // implement node event listeners here
        },
    },
})

## Note: Symbol Rendering

Symbols are generated dynamically by the API's RenderService. They are not saved or retrieved from a database.

---