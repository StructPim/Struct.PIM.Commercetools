declare namespace Cypress {
    interface Chainable {

        ///////////////// Languages /////////////////
        createLanguages();
        ///////////////// Languages /////////////////
        
        ///////////////// Catalogue /////////////////
        // Commercetools
        createCatalogue(value: any): Chainable<Promise>;
        getCatalogueUid(value: number): Chainable<Promise>;
        deleteCatalogue(value: string): Chainable<Promise>;
        // PIM
        getCatalogueUids(): Chainable<Promise>;
        getCatalogues(): Chainable<Promise>;
        getCataloguesChildren(catalogueUid: string): Chainable<Promise>;

        ///////////////// Category /////////////////
        // Commercetools
        createCategories(value: number[]): Chainable<Promise>;
        deleteCategories(categoryIds: number[]): Chainable<Promise>;
        // PIM
        getCategoriesIds(): Chainable<Promise>;
        getCategories(): Chainable<Promise>;
        getProductIdsByCategory(categoryId: number): Chainable<Promise>;

        ///////////////// ProductStructure /////////////////
        // Commercetools
        createProductStructure(productStructureUid: string): Chainable<Promise>;
        getProductStructures(): Chainable<Promise>;
        deleteProductStructure(productStructureUid: string): Chainable<Promise>;
        // PIM
        getProductStructure(productStructureUid: string): Chainable<Promise>;

        ///////////////// Product /////////////////
        // Commercetools
        createProduct(productIds: number[]): Chainable<Promise>;
        getProduct(productIds: number): Chainable<Promise>;
        deleteProduct(productIds: number[]): Chainable<Promise>;
        // PIM
        getProducts(limit: number): Chainable<Promise>;
        getProduct(productId: number): Chainable<Promise>;

        ///////////////// Variant /////////////////
        // Commercetools
        // PIM
        getProductVariants(productId: number): Chainable<Promise>;
        getVariants(variantsIds: number[]): Chainable<Promise>;

        ///////////////// Classification /////////////////
        // Commercetools
        // PIM
        getClassifications(productId: number): Chainable<Promise>;
        
    }
}

