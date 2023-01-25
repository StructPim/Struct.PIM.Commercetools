describe('ProductStructure webhook testing', () => {

    let ProductStructureUid = '';
    let ProductStructureAlias = '';
    before(() => {
        cy.getProductStructures().then(response => {
            ProductStructureUid = response.body[2].Uid;
            ProductStructureAlias = response.body[2].Alias;
        });
    });

    describe('create', () => {
        it('success', () => {
            cy.request({
                method: 'POST',
                url: 'productstructure/webhook',
                headers: {
                    'X-Event-Key': 'productstructures:created',
                    "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                },
                body: {
                    ProductStructureUid,
                    ProductStructureAlias,
                },
            }).then(response => {
                expect(response.status).to.equal(200);
            });
        });
        it('fails -> product structure exists', () => {
            cy.request({
                method: 'POST',
                url: 'productstructure/webhook',
                headers: {
                    'X-Event-Key': 'productstructures:created',
                    "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                },
                body: {
                    ProductStructureUid,
                    ProductStructureAlias,
                },
                failOnStatusCode: false,
            }).then(response => {
                expect(response.status).to.equal(400);
            });
        });
        it('fails', () => {
            cy.request({
                method: 'POST',
                url: 'productstructure/webhook',
                headers: {
                    'X-Event-Key': 'productstructures:created',
                    "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                },
                body: {},
                failOnStatusCode: false,
            }).then(response => {
                expect(response.status).to.equal(400);
            });
        });
    });
    describe('updates', () => {
        it("fails", () => {
            cy.request({
                method: 'POST',
                url: 'productstructure/webhook',
                headers: {
                    'X-Event-Key': 'productstructures:updated',
                    "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                },
                body: {
                    ProductStructureUid,
                    ProductStructureAlias,
                },
                failOnStatusCode: false,
            }).then(response => {
                expect(response.status).to.equal(400);
            });
        });

    });
    describe('delete', () => {
        it('success', () => {
            cy.deleteProductStructure(ProductStructureUid).then(response => {
                expect(response.status).to.equal(200);
            });
        });
    });

    it('fails since there is no webhook handler', () => {
        let xEventKey = 'productstructures:canthandle';
        cy.request({
            method: 'POST',
            url: 'productstructure/webhook',
            headers: {
                'X-Event-Key': xEventKey,
                "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
            },
            body: {
                ProductStructureUid,
                ProductStructureAlias,
            },
            failOnStatusCode: false,
        }).then(response => {
            expect(response.body).to.contain(`No handler for webhook ${xEventKey}`);
            expect(response.status).to.equal(400);
        });
    });
});