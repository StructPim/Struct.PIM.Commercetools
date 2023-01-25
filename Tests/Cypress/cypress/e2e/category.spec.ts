describe('Category webhook testing', () => {
    let categoryIds: number[] = new Array<number>();
    let catalogueUids: string[] = new Array<string>();
    before(() => {
        cy.getCatalogues().then(resp => {
            Promise.all(resp.body.map((r: any) => {
                cy.createCatalogue(r);
                catalogueUids.push(r.Uid);
            })).then(() => {
                cy.getCategories().then(categoriesResp => {
                    categoryIds = categoriesResp.body.Categories.map((category: any) => category.Id);
                });
            });
        });
    });
    after(()=>{
        catalogueUids.map((c:string)=>cy.deleteCatalogue(c));
    });
    describe('create', () => {
        it('success', () => {
            cy.request({
                method: 'POST',
                url: 'category/webhook',
                headers: {
                    'X-Event-Key': 'categories:created',
                    "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                },
                body: {
                    categoryIds: categoryIds,
                },
            }).then(response => {
                expect(response.status).to.equal(200);
            });
        });
        it('fails', () => {
            cy.request({
                method: 'POST',
                url: 'category/webhook',
                headers: {
                    'X-Event-Key': 'categories:created',
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
        it("success", () => {
            cy.request({
                method: 'POST',
                url: 'category/webhook',
                headers: {
                    'X-Event-Key': 'categories:updated',
                    "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                },
                body: {
                    categoryIds: categoryIds,
                },
            }).then(response => {
                expect(response.status).to.equal(200);
            });
        });

    });
    describe('delete', () => {
        it('success', () => {
            cy.deleteCategories(categoryIds).then(response => {
                expect(response.status).to.equal(200);
            });

        });
    });

    it('fails since there is no webhook handler', () => {
        let xEventKey = 'categories:canthandle';
        cy.request({
            method: 'POST',
            url: 'category/webhook',
            headers: {
                'X-Event-Key': xEventKey,
                "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
            },
            body: {
                categoryIds: categoryIds,
            },
            failOnStatusCode: false,
        }).then(response => {
            expect(response.body).to.contain(`No handler for webhook ${xEventKey}`);
            expect(response.status).to.equal(400);
        });
    });
});