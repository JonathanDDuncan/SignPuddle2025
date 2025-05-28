import { mount } from 'cypress/svelte'
import '../../src/app.css'

Cypress.Commands.add('mount', mount)

// Mock stores for component testing
beforeEach(() => {
  cy.window().then((win) => {
    win.localStorage.clear()
  })
})