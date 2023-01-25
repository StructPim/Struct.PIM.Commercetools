import { defineConfig } from 'cypress'

export default defineConfig({
  pimApiKey: 'f4dd36b1-d83c-4641-a75a-36c9066624d3',
  pimApiUrl: 'http://commercetools_api_dev.falsk.dk',
  commerceToolsAccApiKey: 'W279!ORV5gMT@cnhV0GW&n5@!g864',
  e2e: {
    // We've imported your old cypress plugins here.
    // You may want to clean this up later by importing these.
    setupNodeEvents(on, config) {
      return require('./cypress/plugins/index.js')(on, config)
    },
    baseUrl: 'http://localhost:5222',
    specPattern: 'cypress/e2e/**/*.spec.ts',
  },
})
