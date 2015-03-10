define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/auditMessageDetails.html',
    'moment'
], function(Backbone, _, $, template, moment) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        bindings: {
            '.TypeName': 'TypeName',
            '.FullTypeName': 'FullTypeName',
            '.MessageType': 'MessageType',
            '.ConsumerType': 'ConsumerType',
            '.Language': 'Language',
            '.SourceMachine': 'SourceMachine',
            '.SourceAddress': 'SourceAddress',
            '.DestinationMachine': 'DestinationMachine',
            '.DestinationAddress': 'DestinationAddress',
            '.TimeSent': {
                observe: 'TimeSent',
                onGet: function(value) {
                    return moment.utc(value).format("DD/MM/YYYY HH:mm:ss.SSS");
                }
            },
            '.TimeReceived': {
                observe: 'TimeReceived',
                onGet: function(value) {
                    return moment.utc(value).format("DD/MM/YYYY HH:mm:ss.SSS");
                }
            },
            '.TimeProcessed': {
                observe: 'TimeProcessed',
                onGet: function(value) {
                    return moment.utc(value).format("DD/MM/YYYY HH:mm:ss.SSS");
                }
            },
            '.ProcessingTime': {
                observe: ['TimeReceived', 'TimeProcessed'],
                onGet: function(values) {
                    return moment.utc(moment.duration(moment.utc(values[0]).diff(moment.utc(values[1]))).get("milliseconds")).format("HH:mm:ss.SSS");
                }
            }
        },

        initialize: function() {
            _.bindAll(this);
        },

        _jsonSyntaxHighlight: function(json) {
            if (typeof json !== 'string') {
                json = JSON.stringify(json, undefined, 2);
            }

            json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

            return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function(match) {
                var cls = 'number';
                if (/^"/.test(match)) {
                    if (/:$/.test(match)) {
                        cls = 'key';
                    } else {
                        cls = 'string';
                    }
                } else if (/true|false/.test(match)) {
                    cls = 'boolean';
                } else if (/null/.test(match)) {
                    cls = 'null';
                }
                return '<span class="' + cls + '">' + match + '</span>';
            });
        },

        render: function() {
            this.$el.html(template);
            this.stickit();
            var jsonObject = JSON.parse(this.model.get("Body"));
            var prittyJson = JSON.stringify(jsonObject, undefined, 2);
            this.$el.find(".Body").html(this._jsonSyntaxHighlight(prittyJson));
            return this;
        },

        onClose: function() {
            this.unstickit();
        }
    });

    return view;
});
