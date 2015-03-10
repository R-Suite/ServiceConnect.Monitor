define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/exceptionModal.html'
], function(Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({

        events: {
            "click .closeModal": "_closeModal"
        },

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            var prittyJson = JSON.stringify(this.model.get("Exception"), undefined, 2);
            this.$el.find(".exception").html(this._jsonSyntaxHighlight(prittyJson));
            this.$el.find(".exceptionModal").modal('show');
            this.$el.find(".exceptionModal").on('hidden.bs.modal', this.closeView);
            return this;
        },

        _closeModal: function() {
            this.$el.find(".exceptionModal").modal('hide');
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

        closeView: function() {
            this.close();
        },

        onClose: function() {
            this.$el.find(".exceptionModal").off('hidden.bs.modal', this.closeView);
            this.remove();
        }
    });

    return view;
});
