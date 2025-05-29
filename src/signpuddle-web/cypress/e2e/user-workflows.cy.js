// describe('User Workflows - Complete User Journeys', () => {
//   beforeEach(() => {
//     cy.interceptApiCalls()
//   })

//   it('should complete full sign creation workflow', () => {
//     cy.loginUser()
    
//     // Start from home
//     cy.visit('/')
//     cy.get('[data-cy=create-sign-btn]').click()
    
//     // Should navigate to sign maker
//     cy.url().should('include', '/signmaker')
    
//     // Create a complex sign
//     cy.selectSymbol('S10000') // Hand
//     cy.get('[data-cy=sign-canvas]').click(200, 200)
    
//     cy.selectSymbol('S26500') // Movement
//     cy.get('[data-cy=sign-canvas]').click(250, 180)
    
//     cy.selectSymbol('S30000') // Contact
//     cy.get('[data-cy=sign-canvas]').click(220, 220)
    
//     // Save the sign
//     cy.saveSign({
//       term: 'Complex Sign',
//       definition: 'A sign with multiple components'
//     })
    
//     // Verify save was successful
//     cy.wait('@saveSign')
//     cy.get('[data-cy=save-success-message]').should('contain', 'Sign saved successfully')
    
//     // Navigate to dictionary to verify sign exists
//     cy.get('[data-cy=nav-dictionary]').click()
//     cy.get('[data-cy=search-input]').type('Complex Sign')
//     cy.get('[data-cy=search-btn]').click()
//     cy.get('[data-cy=sign-grid] .sign-entry').should('contain', 'Complex Sign')
//   })

//   it('should create and edit sign text document', () => {
//     cy.loginUser()
    
//     // Navigate to sign text editor
//     cy.visit('/signtext')
    
//     // Create a sentence with multiple signs
//     const signs = [
//       { symbol: 'S10000', term: 'Hello' },
//       { symbol: 'S20000', term: 'My' },
//       { symbol: 'S30000', term: 'Name' },
//       { symbol: 'S40000', term: 'Is' }
//     ]
    
//     signs.forEach((sign, index) => {
//       cy.get('[data-cy=add-sign-btn]').click()
//       cy.selectSymbol(sign.symbol)
//       cy.get('[data-cy=sign-canvas]').click(200, 200)
//       cy.get('[data-cy=sign-term-input]').clear().type(sign.term)
//       cy.get('[data-cy=confirm-sign-btn]').click()
//     })
    
//     // Verify all signs are in sequence
//     cy.get('[data-cy=sign-sequence] .sign-item').should('have.length', 4)
    
//     // Edit the second sign
//     cy.get('[data-cy=sign-sequence] .sign-item').eq(1).find('[data-cy=edit-sign-btn]').click()
//     cy.get('[data-cy=sign-term-input]').clear().type('Your')
//     cy.get('[data-cy=confirm-sign-btn]').click()
    
//     // Verify edit was applied
//     cy.get('[data-cy=sign-sequence] .sign-item').eq(1).should('contain', 'Your')
    
//     // Save the document
//     cy.get('[data-cy=save-text-btn]').click()
//     cy.get('[data-cy=text-title-input]').type('Introduction Sentence')
//     cy.get('[data-cy=confirm-save-text-btn]').click()
    
//     cy.wait('@saveSign')
//     cy.get('[data-cy=save-success-message]').should('be.visible')
//   })

//   it('should search and modify existing signs', () => {
//     cy.loginUser()
    
//     // Start from dictionary
//     cy.visit('/dictionary')
//     cy.wait('@getDictionaries')
    
//     // Search for existing sign
//     cy.get('[data-cy=search-input]').type('hello')
//     cy.get('[data-cy=search-btn]').click()
    
//     // Select a sign to edit
//     cy.get('[data-cy=sign-grid] .sign-entry').first().click()
//     cy.get('[data-cy=edit-sign-btn]').click()
    
//     // Should navigate to sign maker with loaded sign
//     cy.url().should('include', '/signmaker')
//     cy.get('[data-cy=sign-canvas] .symbol').should('exist')
    
//     // Modify the sign
//     cy.selectSymbol('S26500')
//     cy.get('[data-cy=sign-canvas]').click(300, 200)
    
//     // Save modified version
//     cy.get('[data-cy=save-as-new-btn]').click()
//     cy.get('[data-cy=sign-term-input]').clear().type('Hello Modified')
//     cy.get('[data-cy=confirm-save-btn]').click()
    
//     cy.wait('@saveSign')
//     cy.get('[data-cy=save-success-message]').should('be.visible')
//   })
// })