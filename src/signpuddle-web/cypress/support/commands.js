// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************

// -- This is a parent command --
// Cypress.Commands.add('login', (email, password) => { ... })

// -- This is a child command --
// Cypress.Commands.add('drag', { prevSubject: 'element'}, (subject, options) => { ... })

Cypress.Commands.add('interceptApiCalls', () => {
  cy.intercept('GET', '/api/symbols/**', { fixture: 'symbols.json' }).as('getSymbols')
  cy.intercept('GET', '/api/signs/**', { fixture: 'signs.json' }).as('getSigns')
  cy.intercept('POST', '/api/signs', { fixture: 'signSaveResponse.json' }).as('saveSign')
  cy.intercept('PUT', '/api/signs/**', { fixture: 'signUpdateResponse.json' }).as('updateSign')
  cy.intercept('DELETE', '/api/signs/**', { statusCode: 204 }).as('deleteSign')
})

Cypress.Commands.add('selectSymbol', (symbolKey) => {
  cy.get('[data-cy=symbol-palette]').should('be.visible')
  cy.get(`[data-symbol-key="${symbolKey}"]`).click()
})

Cypress.Commands.add('createBasicSign', () => {
  cy.visit('/signmaker')
  cy.interceptApiCalls()
  
  // Select a hand symbol
  cy.selectSymbol('S10000')
  
  // Place it on canvas
  cy.get('[data-cy=sign-canvas]').click(200, 200)
  
  // Verify sign is created
  cy.get('[data-cy=current-sign]').should('contain', 'S10000')
})

Cypress.Commands.add('saveSign', (signData = {}) => {
  const defaultData = {
    term: 'Test Sign',
    definition: 'A test sign for Cypress',
    ...signData
  }
  
  cy.get('[data-cy=save-sign-btn]').click()
  cy.get('[data-cy=sign-term-input]').clear().type(defaultData.term)
  cy.get('[data-cy=sign-definition-input]').clear().type(defaultData.definition)
  cy.get('[data-cy=confirm-save-btn]').click()
})

Cypress.Commands.add('loginUser', (username = 'testuser', password = 'testpass') => {
  cy.window().then((win) => {
    win.localStorage.setItem('signpuddle_user', JSON.stringify({
      username,
      isAuthenticated: true,
      token: 'mock-jwt-token'
    }))
  })
})

// -- This is a dual command --
// Cypress.Commands.add('dismiss', { prevSubject: 'optional'}, (subject, options) => { ... })

Cypress.Commands.add('stubSignRequests', () => {
  cy.fixture('signUpdateResponse.json').then((response) => {
    cy.intercept('POST', '**/api/signs/**', response).as('signUpdate')
    cy.intercept('PUT', '**/api/signs/**', response).as('signCreate')
    cy.intercept('GET', '**/api/signs/**', response).as('signGet')
  })
})

// -- This will overwrite an existing command --
// Cypress.Commands.overwrite('visit', (originalFn, url, options) => { ... })