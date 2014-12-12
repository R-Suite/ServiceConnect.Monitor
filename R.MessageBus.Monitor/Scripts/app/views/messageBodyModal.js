define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/messageBodyModal.html'
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
            var jsonObject = JSON.parse(this.model.get("Body"));
            var prittyJson = JSON.stringify(jsonObject, undefined, 2);
            this.$el.find(".body").html(this._jsonSyntaxHighlight(prittyJson));
            this.$el.find(".messageBodyModal").modal('show');
            this.$el.find(".messageBodyModal").on('hidden.bs.modal', this.closeView);
            return this;
        },

        _closeModal: function() {
            this.$el.find(".messageBodyModal").modal('hide');
        },

        closeView: function() {
            this.close();
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

        onClose: function() {
            this.$el.find(".messageBodyModal").off('hidden.bs.modal', this.closeView);
            this.remove();
        }
    });

    return view;
});
