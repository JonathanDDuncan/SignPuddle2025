// describe('SignMaker - Sign Creation and Editing', () => {
//   beforeEach(() => {
//     cy.interceptApiCalls()
//     cy.visit('/signmaker')
//   })

//   it('should load the sign maker interface', () => {
//     cy.get('[data-cy=sign-canvas]').should('be.visible')
//     cy.get('[data-cy=symbol-palette]').should('be.visible')
//     cy.get('[data-cy=tool-palette]').should('be.visible')
//   })

//   it('should create a new sign with symbols', () => {
//     // Select and place a hand symbol
//     cy.selectSymbol('S10000')
//     cy.get('[data-cy=sign-canvas]').click(200, 200)
    
//     // Select and place movement symbol
//     cy.selectSymbol('S26500')
//     cy.get('[data-cy=sign-canvas]').click(250, 180)
    
//     // Verify symbols are placed
//     cy.get('[data-cy=current-sign]').should('contain', 'S10000')
//     cy.get('[data-cy=current-sign]').should('contain', 'S26500')
    
//     // Check canvas has symbols
//     cy.get('[data-cy=sign-canvas] .symbol').should('have.length', 2)
//   })

//   it('should manipulate symbols (rotate, flip, resize)', () => {
//     cy.createBasicSign()
    
//     // Select first symbol
//     cy.get('[data-cy=sign-canvas] .symbol').first().click()
    
//     // Rotate symbol
//     cy.get('[data-cy=rotate-btn]').click()
//     cy.get('[data-cy=current-sign]').should('match', /[0-9]{3}[1-7][0-9]/)
    
//     // Flip symbol
//     cy.get('[data-cy=flip-btn]').click()
//     cy.get('[data-cy=current-sign]').should('match', /[0-9]{3}[0-9][1-7]/)
//   })

//   it('should save a sign to dictionary', () => {
//     cy.loginUser()
//     cy.createBasicSign()
    
//     cy.saveSign({
//       term: 'Hello',
//       definition: 'A greeting sign'
//     })
    
//     cy.wait('@saveSign').then((interception) => {
//       expect(interception.request.body).to.include({
//         term: 'Hello',
//         definition: 'A greeting sign'
//       })
//     })
    
//     cy.get('[data-cy=save-success-message]').should('be.visible')
//   })

//   it('should handle symbol search and filtering', () => {
//     cy.get('[data-cy=symbol-search]').type('hand')
//     cy.get('[data-cy=symbol-palette] .symbol').should('have.length.greaterThan', 0)
    
//     // Filter by category
//     cy.get('[data-cy=category-filter]').select('hand')
//     cy.get('[data-cy=symbol-palette] .symbol').each(($el) => {
//       cy.wrap($el).should('have.attr', 'data-category', 'hand')
//     })
//   })

//   it('should undo and redo operations', () => {
//     cy.createBasicSign()
    
//     // Add another symbol
//     cy.selectSymbol('S26500')
//     cy.get('[data-cy=sign-canvas]').click(300, 200)
    
//     cy.get('[data-cy=sign-canvas] .symbol').should('have.length', 2)
    
//     // Undo last action
//     cy.get('[data-cy=undo-btn]').click()
//     cy.get('[data-cy=sign-canvas] .symbol').should('have.length', 1)
    
//     // Redo action
//     cy.get('[data-cy=redo-btn]').click()
//     cy.get('[data-cy=sign-canvas] .symbol').should('have.length', 2)
//   })
// })