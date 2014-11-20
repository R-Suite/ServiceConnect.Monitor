define(['backbone', 'underscore', 'backgrid'], function(Backbone, _, Backgrid) {

    "use strict";

    Backgrid.cell_initialize_original = Backgrid.Cell.prototype.initialize;
    Backgrid.Cell.prototype.initialize = function() {
        Backgrid.cell_initialize_original.apply(this, arguments);
        var className = this.column.get('className');
        if (className) this.$el.addClass(className);
        if (this.column.attributes.handler) {
            this.column.attributes.handler(this.model, this.$el);
        }
    };

    Backgrid.HtmlCell = Backgrid.Cell.extend({
        className: "html-cell",
        render: function() {
            this.$el.empty();
            this.$el.html(this.formatter.fromRaw(this.model.get(this.column.get("name")), this.model));
            this.delegateEvents();
            return this;
        }
    });

    return Backbone;
});
