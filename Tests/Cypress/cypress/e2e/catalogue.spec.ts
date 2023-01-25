describe('Catalogue webhook testing', () => {
    let masterCatalogues: Array<{ Uid: string }> = new Array<{ Uid: '' }>();
    before(() => {
        cy.getCatalogues().then(resp => {
            masterCatalogues = resp.body.filter((mc: any) => mc.IsMaster);
        });
    });
    describe('create', () => {
        it('success', () => {
            masterCatalogues.map(masterCatalogue =>
                cy.createCatalogue(masterCatalogue).then(response => {
                    expect(response.status).to.equal(200);
                })
            );
            it('fails', () => {
                cy.request({
                
                    method: 'POST',
                    url: 'catalogue/webhook',
                    headers: {
                        'X-Event-Key': 'catalogues:created',
                        "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                    },
                    body: {},
                    failOnStatusCode: false,
                }).then(response => {
                    expect(response.status).to.equal(424);
                });
            });
        });
    });
    describe('updates', () => {
        it("success", () => {
            masterCatalogues.map(masterCatalogue =>
                cy.request({
                    method: 'POST',
                    url: 'catalogue/webhook',
                    headers: {
                        'X-Event-Key': 'catalogues:updated',
                        "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                    },
                    body: {
                        'CatalogueUid': masterCatalogue.Uid,
                        'CatalogueAlias': masterCatalogue.Uid,
                    },
                }).then(response => {
                    expect(response.status).to.equal(200);
                })
            );
        });

    });
    describe('delete', () => {
        it('success', () => {
            masterCatalogues.map(masterCatalogue =>
                cy.deleteCatalogue(masterCatalogue.Uid).then(response => {
                    expect(response.status).to.equal(200);
                })
            );
        });
    });

    it('fails if there is no webhook handler', () => {
        let xEventKey = 'catalogues:canthandle';
        cy.request({
            method: 'POST',
            url: 'catalogue/webhook',
            headers: {
                'X-Event-Key': xEventKey,
                "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
            },
            body: {
                'CatalogueUid': '749F14DA-940B-4AA2-8AE9-7768F4AF04EC',
                'CatalogueAlias': 'CatalogueAlias_Updated',
            },
            failOnStatusCode: false,
        }).then(response => {
            expect(response.body).to.contain(`No handler for webhook ${xEventKey}`);
            expect(response.status).to.equal(400);
        });
    });
});