// describe('Dictionary - Browse and Search Signs', () => {
//   beforeEach(() => {
//     cy.intercept('GET', '/api/dictionaries', { fixture: 'dictionaries.json' }).as('getDictionaries')
//     cy.intercept('GET', '/api/dictionaries/*/signs', { fixture: 'dictionarySigns.json' }).as('getDictionarySigns')
//     cy.visit('/dictionary')
//   })

//   it('should load dictionary browser', () => {
//     cy.wait('@getDictionaries')
//     cy.get('[data-cy=dictionary-list]').should('be.visible')
//     cy.get('[data-cy=search-input]').should('be.visible')
//     cy.get('[data-cy=filter-options]').should('be.visible')
//   })

//   it('should browse dictionary entries', () => {
//     cy.wait('@getDictionaries')
    
//     // Select a dictionary
//     cy.get('[data-cy=dictionary-list] .dictionary-item').first().click()
//     cy.wait('@getDictionarySigns')
    
//     // Verify signs are displayed
//     cy.get('[data-cy=sign-grid] .sign-entry').should('have.length.greaterThan', 0)
    
//     // Click on a sign to view details
//     cy.get('[data-cy=sign-grid] .sign-entry').first().click()
//     cy.get('[data-cy=sign-detail-modal]').should('be.visible')
//     cy.get('[data-cy=sign-term]').should('not.be.empty')
//     cy.get('[data-cy=sign-definition]').should('not.be.empty')
//   })

//   it('should search for signs', () => {
//     cy.wait('@getDictionaries')
    
//     // Search by term
//     cy.get('[data-cy=search-input]').type('hello')
//     cy.get('[data-cy=search-btn]').click()
    
//     cy.get('[data-cy=sign-grid] .sign-entry').each(($el) => {
//       cy.wrap($el).should('contain.text', 'hello')
//     })
//   })

//   it('should filter signs by category', () => {
//     cy.wait('@getDictionaries')
//     cy.get('[data-cy=dictionary-list] .dictionary-item').first().click()
//     cy.wait('@getDictionarySigns')
    
//     // Apply category filter
//     cy.get('[data-cy=category-filter]').select('greetings')
//     cy.get('[data-cy=apply-filter-btn]').click()
    
//     cy.get('[data-cy=sign-grid] .sign-entry').each(($el) => {
//       cy.wrap($el).should('have.attr', 'data-category', 'greetings')
//     })
//   })

//   it('should handle pagination', () => {
//     cy.wait('@getDictionaries')
//     cy.get('[data-cy=dictionary-list] .dictionary-item').first().click()
//     cy.wait('@getDictionarySigns')
    
//     // Check pagination controls
//     cy.get('[data-cy=pagination]').should('be.visible')
    
//     // Go to next page
//     cy.get('[data-cy=next-page-btn]').click()
//     cy.url().should('include', 'page=2')
    
//     // Go back to previous page
//     cy.get('[data-cy=prev-page-btn]').click()
//     cy.url().should('include', 'page=1')
//   })

//   it('should copy signs to personal dictionary', () => {
//     cy.loginUser()
//     cy.wait('@getDictionaries')
//     cy.get('[data-cy=dictionary-list] .dictionary-item').first().click()
//     cy.wait('@getDictionarySigns')
    
//     // Select a sign and copy it
//     cy.get('[data-cy=sign-grid] .sign-entry').first().click()
//     cy.get('[data-cy=copy-sign-btn]').click()
//     cy.get('[data-cy=select-target-dictionary]').select('My Dictionary')
//     cy.get('[data-cy=confirm-copy-btn]').click()
    
//     cy.wait('@saveSign')
//     cy.get('[data-cy=copy-success-message]').should('be.visible')
//   })
// })