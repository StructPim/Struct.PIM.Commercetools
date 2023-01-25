function createProductStructure(productStructureUids: string[], productStructureUid: string): Cypress.Chainable<any> {
    if (productStructureUids.indexOf(productStructureUid) === -1) {
        productStructureUids.push(productStructureUid);
        return cy.createProductStructure(productStructureUid);
    }
    return new Cypress.Promise((resolve: any, reject: any) => {
        resolve();
    });
}

let items: Array<{ product: any, classifications: any }> = new Array<{ product: any, variants: any, classifications: any }>();

function getProductIds(limit: number = 2): Array<number> {
    return items.slice(0, 2).map(p => p.product.Id);
}

describe('Product webhook testing', () => {
    let masterCatalogue: { Uid: string } = {Uid: ''};
    let categoryIds: number[] = new Array<number>();
    let catalogueUids: string[] = new Array<string>();
    const productStructureUids: Array<string> = new Array<string>();
    let variantsIds = new Array<number>();
    before(() => {
        cy.getCatalogues().then(resp => {
            masterCatalogue = resp.body.filter((mc: any) => mc.IsMaster)[0];
            Promise.all(resp.body.map((r: any) => {
                cy.createCatalogue(r);
                catalogueUids.push(r.Uid);
            })).then(() => {
                cy.getCategories().then(categoriesResp => {
                    categoryIds = categoriesResp.body.Categories.map((category: any) => category.Id);
                    cy.createCategories(categoryIds).then(() => {
                        categoryIds.forEach(id => {
                            cy.getProductIdsByCategory(id).then((productsIdResponse: any) => {
                                productsIdResponse.body.forEach((id: any) => {
                                    const first = items.find((obj: any) => {
                                        return obj.product.Id === id;
                                    });
                                    if (!first) {
                                        cy.getProduct(id).then(productResponse => {
                                            const productStructureUid = productResponse.body.ProductStructureUid;
                                            createProductStructure(productStructureUids, productStructureUid).then(_ => {
                                                cy.getProductVariants(id).then(variantsIdsResponse => {
                                                    cy.getVariants(variantsIdsResponse.body).then(variants => {
                                                        cy.getClassifications(id).then(classifications => {
                                                            if (variants.body.length > 0) {
                                                                items.push({
                                                                    product: productResponse.body,
                                                                    classifications: classifications.body,
                                                                });
                                                                variantsIds = [...variantsIds, ...variants.body.map((v: any) => v.Id)];
                                                            }
                                                        });
                                                    });
                                                });
                                            });
                                        });
                                    }
                                });
                            });
                        });
                    });
                });
            });
        });
    });


    after(() => {

        Promise.all(productStructureUids.map(productStructureUid => cy.deleteProductStructure(productStructureUid))).then(_ =>
            cy.deleteCategories(categoryIds).then(_ => cy.deleteCatalogue(masterCatalogue.Uid)));
    });


    describe('create', () => {

        it('products', () => {
            getProductIds().forEach(id => {
                cy.request({
                    method: 'POST',
                    url: 'product/webhook',
                    headers: {
                        'X-Event-Key': 'products:created',
                        "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                    },
                    body: {
                        'productIds': [id],
                    }
                }).then(response => {
                    expect(response.status).to.equal(200);
                });
            })
        });
        it('variants', () => {
            getProductIds().forEach(id => {
                cy.getProductVariants(id).then(response => {
                    let variantIds = response.body;
                    variantIds.forEach((id: any) => {
                        cy.request({
                            method: 'POST',
                            url: 'variant/webhook',
                            headers: {
                                'X-Event-Key': 'variants:created',
                                "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                            },
                            body: {
                                'VariantIds': [id],
                            },
                        }).then(response => {
                            expect(response.status).to.equal(200);
                        });
                    });
                });
            });
        });

    });
    describe('update', () => {
        it('products', () => {
            getProductIds().forEach(id => {
                cy.request({
                    method: 'POST',
                    url: 'product/webhook',
                    headers: {
                        'X-Event-Key': 'products:updated',
                        "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                    },
                    body: {
                        'productIds': [id],
                    }
                }).then(response => {
                    expect(response.status).to.equal(200);
                });
            })
        });
        it('variant', () => {
            getProductIds().forEach(id => {
                cy.getProductVariants(id).then(response => {
                    let variantIds = response.body;
                    variantIds.forEach((id: any) => {
                        cy.request({
                            method: 'POST',
                            url: 'variant/webhook',
                            headers: {
                                'X-Event-Key': 'variants:updated',
                                "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                            },
                            body: {
                                'VariantIds': [id],
                            },
                        }).then(response => {
                            expect(response.status).to.equal(200);
                        });
                    });
                });
            });
        });
    });

    describe('delete', () => {
        it('variants', () => {
            getProductIds().forEach(productId => {
                cy.getProductVariants(productId).then(response => {
                    let variantIds = response.body;
                    variantIds.forEach((id: any) => {
                        cy.request({
                            method: 'POST',
                            url: 'variant/webhook',
                            headers: {
                                'X-Event-Key': 'variants:deleted',
                                "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                            },
                            body: {
                                'VariantIds': [id],
                            },
                        }).then(response => {
                            expect(response.status).to.equal(200);
                        });
                    });
                });
            });
        });
        it('products', () => {
            getProductIds().forEach(id => {
                cy.request({
                    method: 'POST',
                    url: 'product/webhook',
                    headers: {
                        'X-Event-Key': 'products:deleted',
                        "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
                    },
                    body: {
                        'productIds': [id],
                    }
                }).then(response => {
                    expect(response.status).to.equal(200);
                });
            })
        });
    });
});