module.exports = {
    root: true,
    overrides: [
        {
            files: ["cypress/**/*.ts", "cypress/**/*.d.ts"],
            extends: ["plugin:cypress/recommended"],
            plugins: ["cypress"],
        },
        {
            files: ["*.ts", "*.d.ts"],
            extends: ["eslint:recommended", "prettier"],
            parser: "@typescript-eslint/parser",
            parserOptions: {
                project: ["tsconfig.eslint.json", "cypress/tsconfig.cypress.json"],
                tsconfigRootDir: __dirname,
                ecmaVersion: 2020,
                sourceType: "module",
            },

            rules: {
                semi: ["error", "always"],
                "comma-dangle": ["warn", "always-multiline"],
                "object-curly-spacing": ["warn", "always"],
                "arrow-parens": ["warn", "as-needed"],
                "max-len": ["warn", {code: 160, tabWidth: 2}],
                "no-var": "error",
                "no-useless-return": "warn",
                eqeqeq: ["warn", "smart"],
                "no-implicit-coercion": "warn",
                "no-unused-vars": "off",
                "no-case-declarations": "warn",
            },
        },
    ],
};
