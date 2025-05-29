// describe('SignText - Text Editing and Management', () => {
//   beforeEach(() => {
//     cy.interceptApiCalls()
//     cy.visit('/signtext')
//   })

//   it('should load the sign text editor', () => {
//     cy.get('[data-cy=sign-text-editor]').should('be.visible')
//     cy.get('[data-cy=sign-sequence]').should('be.visible')
//     cy.get('[data-cy=add-sign-btn]').should('be.visible')
//   })

//   it('should add signs to text sequence', () => {
//     // Create first sign
//     cy.get('[data-cy=add-sign-btn]').click()
//     cy.selectSymbol('S10000')
//     cy.get('[data-cy=sign-canvas]').click(200, 200)
//     cy.get('[data-cy=confirm-sign-btn]').click()
    
//     // Verify sign added to sequence
//     cy.get('[data-cy=sign-sequence] .sign-item').should('have.length', 1)
    
//     // Add second sign
//     cy.get('[data-cy=add-sign-btn]').click()
//     cy.selectSymbol('S20000')
//     cy.get('[data-cy=sign-canvas]').click(200, 200)
//     cy.get('[data-cy=confirm-sign-btn]').click()
    
//     cy.get('[data-cy=sign-sequence] .sign-item').should('have.length', 2)
//   })

//   it('should edit existing signs in sequence', () => {
//     // Add a sign first
//     cy.get('[data-cy=add-sign-btn]').click()
//     cy.selectSymbol('S10000')
//     cy.get('[data-cy=sign-canvas]').click(200, 200)
//     cy.get('[data-cy=confirm-sign-btn]').click()
    
//     // Edit the sign
//     cy.get('[data-cy=sign-sequence] .sign-item').first().find('[data-cy=edit-sign-btn]').click()
//     cy.selectSymbol('S26500')
//     cy.get('[data-cy=sign-canvas]').click(250, 180)
//     cy.get('[data-cy=confirm-sign-btn]').click()
    
//     // Verify sign was updated
//     cy.get('[data-cy=sign-sequence] .sign-item').first().should('contain', 'S26500')
//   })

//   it('should delete signs from sequence', () => {
//     // Add two signs
//     cy.get('[data-cy=add-sign-btn]').click()
//     cy.selectSymbol('S10000')
//     cy.get('[data-cy=sign-canvas]').click(200, 200)
//     cy.get('[data-cy=confirm-sign-btn]').click()
    
//     cy.get('[data-cy=add-sign-btn]').click()
//     cy.selectSymbol('S20000')
//     cy.get('[data-cy=sign-canvas]').click(200, 200)
//     cy.get('[data-cy=confirm-sign-btn]').click()
    
//     cy.get('[data-cy=sign-sequence] .sign-item').should('have.length', 2)
    
//     // Delete first sign
//     cy.get('[data-cy=sign-sequence] .sign-item').first().find('[data-cy=delete-sign-btn]').click()
//     cy.get('[data-cy=confirm-delete-btn]').click()
    
//     cy.get('[data-cy=sign-sequence] .sign-item').should('have.length', 1)
//   })

//   it('should reorder signs in sequence', () => {
//     // Add multiple signs
//     const signs = ['S10000', 'S20000', 'S30000']
//     signs.forEach((symbolKey) => {
//       cy.get('[data-cy=add-sign-btn]').click()
//       cy.selectSymbol(symbolKey)
//       cy.get('[data-cy=sign-canvas]').click(200, 200)
//       cy.get('[data-cy=confirm-sign-btn]').click()
//     })
    
//     // Drag and drop to reorder
//     cy.get('[data-cy=sign-sequence] .sign-item').first().as('firstSign')
//     cy.get('[data-cy=sign-sequence] .sign-item').last().as('lastSign')
    
//     cy.get('@firstSign').trigger('dragstart')
//     cy.get('@lastSign').trigger('drop')
    
//     // Verify order changed
//     cy.get('[data-cy=sign-sequence] .sign-item').last().should('contain', 'S10000')
//   })

//   it('should save sign text document', () => {
//     cy.loginUser()
    
//     // Create a text with multiple signs
//     const signs = ['S10000', 'S20000']
//     signs.forEach((symbolKey) => {
//       cy.get('[data-cy=add-sign-btn]').click()
//       cy.selectSymbol(symbolKey)
//       cy.get('[data-cy=sign-canvas]').click(200, 200)
//       cy.get('[data-cy=confirm-sign-btn]').click()
//     })
    
//     // Save document
//     cy.get('[data-cy=save-text-btn]').click()
//     cy.get('[data-cy=text-title-input]').type('My Sign Text')
//     cy.get('[data-cy=text-description-input]').type('A sample sign text document')
//     cy.get('[data-cy=confirm-save-text-btn]').click()
    
//     cy.wait('@saveSign')
//     cy.get('[data-cy=save-success-message]').should('be.visible')
//   })
// })