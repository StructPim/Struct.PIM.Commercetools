describe('Languages webhook testing', () => {
    describe('create', () => {
        it('success', () => {
            cy.createLanguages().then((response: any) => {
                expect(response.status).to.equal(200);
            });
        });
    });
});