define(['backbone',
        'underscore',
        'jquery',
        'bower_components/requirejs-text/text!app/templates/endPointGraph.html',
        'vis'
    ], function(Backbone, _, $, template, vis) {

    "use strict";

    var view = Backbone.View.extend({

        initialize: function(options) {
            _.bindAll(this);
            this.endpointCollection = options.endpointCollection;
            this.serviceMessagesCollection = options.serviceMessagesCollection;
        },

        render: function() {
            this.$el.html(template);
            this._buidGraphModel();
            this._renderGraph();
            return this;
        },

        _buidGraphModel: function() {
            var nodes = [];
            this.endpointCollection.each(function(model) {
                nodes.push({
                    id: model.get("Name"),
                    label: model.get("Name")
                });
            });

            var edges = [];
            this.serviceMessagesCollection.each(function(model) {
                edges.push({
                    from: model.get("Out"),
                    to: model.get("In"),
                    label: model.get("Type") + " (" + model.get("Count") + ")"
                });
            });

            this.nodes = nodes;
            this.edges = edges;
        },

        _renderGraph: function() {
            var container = document.getElementById('serviceGraph');
            var data = {
                nodes: this.nodes,
                edges: this.edges
            };
            var options = {
                edges: {
                    style: 'arrow-center',
                    fontSize: 10
                },
                nodes: {
                    shape: 'box',
                    fontSize: 11
                },
                physics: {
                    barnesHut: {
                        enabled: true,
                        springLength: 250,
                        gravitationalConstant: -6400
                    }
                }
            };
            this.network = new vis.Network(container, data, options);
        }
    });

    return view;
});
