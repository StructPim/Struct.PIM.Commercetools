import 'cypress-wait-until';

///////////////// Languages /////////////////
Cypress.Commands.add("createLanguages", () => {
    return cy.request({
        method: 'POST',
        url: 'languages/webhook',
        headers: {
            'X-Event-Key': 'languages:created',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {},
    });
});
///////////////// Languages /////////////////

///////////////// Catalogue /////////////////
// Commercetools
Cypress.Commands.add("createCatalogue", (catalogue: any) => {
    return cy.request({
        method: 'POST',
        url: 'catalogue/webhook',
        headers: {
            'X-Event-Key': 'catalogues:created',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {
            'CatalogueUid': catalogue.Uid,
            'CatalogueAlias': catalogue.Alias,
        },
    });
});
Cypress.Commands.add("getCatalogueUid", (value: number) => {
    return cy.request({
        method: 'GET',
        url: `category?categoryId=${value}`,
        headers: {
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
    });
});
Cypress.Commands.add("deleteCatalogue", (catalogueUid: string) => {
    return cy.request({
        method: 'POST',
        url: 'catalogue/webhook',
        headers: {
            'X-Event-Key': 'catalogues:deleted',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {
            'CatalogueUid': catalogueUid,
        },
    });
});
// PIM
Cypress.Commands.add('getCatalogueUids', () => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/catalogues/uids`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
Cypress.Commands.add('getCatalogues', () => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/catalogues`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
Cypress.Commands.add('getCataloguesChildren', (catalogueUid: string) => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/catalogues/${catalogueUid}/children`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
///////////////// Catalogue /////////////////

///////////////// Category /////////////////
// Commercetools
Cypress.Commands.add("createCategories", (categoryIds: number[]) => {
    return cy.request({
        method: 'POST',
        url: 'category/webhook',
        headers: {
            'X-Event-Key': 'categories:created',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {
            categoryIds: categoryIds,
        },
    });

});
Cypress.Commands.add("deleteCategories", (categoryIds: number[]) => {
    return cy.request({
        method: 'POST',
        url: 'category/webhook',
        headers: {
            'X-Event-Key': 'categories:deleted',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {
            categoryIds: categoryIds,
        },
    });
});
// PIM
Cypress.Commands.add('getCategoriesIds', () => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/categories/ids`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
Cypress.Commands.add('getCategories', () => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/categories`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});

Cypress.Commands.add('getProductIdsByCategory', (categoryId: number) => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/categories/${categoryId}/products`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
///////////////// Category /////////////////

///////////////// ProductStructure /////////////////
// Commercetools
Cypress.Commands.add("createProductStructure", (productStructureUid: string) => {
    cy.request({
        method: 'POST',
        url: 'productstructure/webhook',
        headers: {
            'X-Event-Key': 'productstructures:created',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {
            "ProductStructureUid": productStructureUid,
        },
    });
});

Cypress.Commands.add("getProductStructure", (productStructureUid: string) => {
    cy.request({
        method: 'GET',
        url: `productstructure/${productStructureUid}`,
        headers: {
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        }
    });
});
Cypress.Commands.add("deleteProductStructure", (productStructureUid: string) => {

    cy.request({
        method: 'POST',
        url: 'productstructure/webhook',
        headers: {
            'X-Event-Key': 'productstructures:deleted',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {
            ProductStructureUid: productStructureUid,
        },
    });
});

// PIM
Cypress.Commands.add('getProductStructures', () => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/productstructures`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
///////////////// ProductStructure /////////////////

///////////////// Product /////////////////
// Commercetools
Cypress.Commands.add("createProduct", (productIds: number[]) => {
    return cy.request({
        method: 'POST',
        url: 'product/webhook',
        headers: {
            'X-Event-Key': 'products:created',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {
            'ProductIds': productIds,
        },
    });
});

Cypress.Commands.add("deleteProduct", (productIds: number[]) => {
    return cy.request({
        method: 'POST',
        url: 'product/webhook',
        headers: {
            'X-Event-Key': 'products:deleted',
            "XApiKey": `${Cypress.config('commerceToolsAccApiKey')}`,
        },
        body: {
            'ProductIds': productIds,
        },
    });
});
// PIM
Cypress.Commands.add('getProducts', (limit: number = 5) => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/products?${limit}`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});

Cypress.Commands.add('getProduct', (productId: number) => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/products/${productId}`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
///////////////// Product /////////////////

///////////////// Variant /////////////////
// Commercetools

// PIM
Cypress.Commands.add('getProductVariants', (productId: number) => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/products/${productId}/variants`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
Cypress.Commands.add('getVariants', (variantsIds: number[]) => {
    const req = {
        method: 'POST',
        url: `${Cypress.config('pimApiUrl')}/variants/batch`,
        body: variantsIds,

        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    };
    return cy.request(req);
});
///////////////// Variant /////////////////

///////////////// Classifications /////////////////

// PIM
Cypress.Commands.add('getClassifications', (productId: number) => {
    return cy.request({
        method: 'GET',
        url: `${Cypress.config('pimApiUrl')}/products/${productId}/classifications`,
        headers: {
            authorization: `${Cypress.config('pimApiKey')}`,
        },
    });
});
///////////////// Classifications /////////////////