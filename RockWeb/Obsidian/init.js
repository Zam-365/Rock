(function () {
    window.Obsidian = window.Obsidian || {};

    Obsidian.Util = {
        getGuid: () => 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : r & 0x3 | 0x8;
            return v.toString(16);
        }),
        isSuccessStatusCode: (statusCode) => statusCode && statusCode / 100 === 2
    };

    Obsidian.Templates = {};
    Obsidian.Elements = {};
    Obsidian.Controls = {};
    Obsidian.Blocks = {};
    Obsidian.Fields = {};

    Obsidian.initializeBlock = function (config) {
        new Vue({
            el: config.rootElement,
            components: {
                RockBlock: Obsidian.Controls.RockBlock
            },
            data() {
                return {
                    config: config
                };
            },
            template: `<RockBlock :config="config" />`
        });
    };
})();
