define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/errorRetryModal.html',
    "toastr"
], function(Backbone, _, $, template, toastr) {

    "use strict";

    var view = Backbone.View.extend({

        events: {
            "click .closeModal": "_closeModal",
            "click .confirm": "_retry"
        },

        initialize: function(options) {
            this.onComplete = options.onComplete;
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            this.$el.find(".errorRetryModal").modal('show');
            this.$el.find(".errorRetryModal").on('hidden.bs.modal', this.closeView);
            return this;
        },

        _closeModal: function() {
            this.$el.find(".errorRetryModal").modal('hide');
        },

        closeView: function() {
            this.close();
        },

        _retry: function() {
            var that = this;

            var callbacks = {
                success: function() {
                    toastr.success("Successfully retried messages");
                    that._closeModal();
                    that.onComplete();
                },
                error: function() {
                    toastr.success("Error retrying messages");
                    that._closeModal();
                }
            };

            if (this.collection) {
                this.collection.retry(callbacks);
            } else {
                this.model.retry(callbacks);
            }
        },

        onClose: function() {
            this.$el.find(".errorRetryModal").off('hidden.bs.modal', this.closeView);
            this.remove();
        }
    });

    return view;
});
