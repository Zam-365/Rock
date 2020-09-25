﻿Obsidian.Controls.RockBlock = {
    props: {
        config: {
            type: Object,
            required: true
        }
    },
    provide() {
        return {
            http: {
                get: this.httpGet,
                post: this.httpPost
            },
            blockAction: this.blockAction
        };
    },
    data() {
        return {
            blockGuid: this.config.blockGuid,
            pageGuid: this.config.pageGuid,
            log: [],
            blockComponent: Obsidian.Blocks[this.config.blockFileIdentifier]
        };
    },
    methods: {
        httpCall(method, url, params, data) {
            this.log.push({
                method,
                timestamp: new Date(),
                url
            });

            return axios({
                method,
                url,
                data,
                params
            });
        },
        httpGet(url, params) {
            return this.httpCall('GET', url, params);
        },
        httpPost(url, params, data) {
            return this.httpCall('POST', url, params, data);
        },
        blockAction(actionName, data) {
            try {
                return this.httpPost(`/api/blocks/action/${this.pageGuid}/${this.blockGuid}/${actionName}`, undefined, data);
            }
            catch (e) {
                if (e.response && e.response.data && e.response.data.Message) {
                    throw e.response.data.Message;
                }

                throw e;
            }
        }
    },
    template: `<component :is="blockComponent" />`
};
