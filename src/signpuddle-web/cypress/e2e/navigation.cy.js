describe('Navigation and Routing', () => {
  // it('should navigate between main sections', () => {
  //   cy.visit('/')
    
  //   // Test home page
  //   cy.get('[data-cy=welcome-message]').should('be.visible')
    
  //   // Navigate to Sign Maker
  //   cy.get('[data-cy=nav-signmaker]').click()
  //   cy.url().should('include', '/signmaker')
  //   cy.get('[data-cy=sign-canvas]').should('be.visible')
    
  //   // Navigate to Sign Text
  //   cy.get('[data-cy=nav-signtext]').click()
  //   cy.url().should('include', '/signtext')
  //   cy.get('[data-cy=sign-text-editor]').should('be.visible')
    
  //   // Navigate to Dictionary
  //   cy.get('[data-cy=nav-dictionary]').click()
  //   cy.url().should('include', '/dictionary')
  //   cy.get('[data-cy=dictionary-list]').should('be.visible')
  // })

  it('should handle browser back/forward navigation', () => {
    cy.visit('/signmaker')
    cy.visit('/signtext')
    cy.visit('/dictionary')
    
    // Go back
    cy.go('back')
    cy.url().should('include', '/signtext')
    
    // Go forward
    cy.go('forward')
    cy.url().should('include', '/dictionary')
  })

  // it('should persist state across navigation', () => {
  //   cy.interceptApiCalls()
  //   cy.visit('/signmaker')
    
  //   // Create a sign
  //   cy.selectSymbol('S10000')
  //   cy.get('[data-cy=sign-canvas]').click(200, 200)
    
  //   // Navigate away and back
  //   cy.get('[data-cy=nav-dictionary]').click()
  //   cy.get('[data-cy=nav-signmaker]').click()
    
  //   // Verify sign is still there
  //   cy.get('[data-cy=sign-canvas] .symbol').should('exist')
  // })
})