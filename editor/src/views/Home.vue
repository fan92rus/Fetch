<template>
  <div class="container">
    <div class="columns">
      <div class="column">
        <div class="field">
          <div class="control">
            <input class="input is-primary" v-model="query" type="text" placeholder="Primary input" />
            <div v-for="item in sorted" :key="item.id" class="columns box mt-3">
              <div class="column">
                <b-checkbox v-bind="item.Selected">Выбрать</b-checkbox>
              </div>
              <div class="column">{{item.Type}}</div>
              <div class="column">{{item.Text}}</div>
              <div class="column">{{item.Selector}}</div>
            </div>
          </div>
        </div>
      </div>
      <div class="column">
        <div class="bg-purple sm:bg-green md:bg-blue md:text-yellow lg:bg-red xl:bg-orange">Test</div>
      </div>
    </div>
  </div>
</template>

<script>
import { Convertor } from "../assets/Serices/DataProcessing";

const converter = new Convertor();

export default {
  name: "TreeExample",
  data: function() {
    return {
      query: "",
      treeDisplayData: {
        Text: "Test",
        Nodes: [
          {
            Type: "Default",
            Selector: "div",
            Attributes: [],
            Classes: [
              "col-xs-12",
              "col-sm-6",
              "col-md-6",
              "col-lg-6",
              "delivery-info-block-2"
            ],
            Nodes: [
              {
                Type: "Text",
                Selector: ":nth-child(2) > span",
                Attributes: [],
                Classes: [],
                Text: "Не нашли нужный населенный пункт?",
                isOpen: true
              },
              {
                Type: "Text",
                Selector: ":nth-child(2) > span",
                Attributes: [],
                Classes: [],
                Text: "Позвоните нам по номеру"
              },
              {
                Type: "Link",
                Selector: ":nth-child(2) > span",
                Attributes: [],
                Classes: [],
                Text: "и мы поможем вам"
              }
            ]
          },
          {
            Type: "Default",
            Selector: "div",
            Attributes: [],
            Classes: ["col-xs-12", "visible-xs", "delivery-info-block-3"],
            Nodes: []
          },
          {
            Type: "Text",
            Selector: "div.row",
            Attributes: [],
            Classes: ["row", "delivery-area"]
          },
          {
            Type: "Text",
            Selector: "div.row",
            Attributes: [],
            Classes: ["row", "delivery-info-area"]
          }
        ]
      }
    };
  },
  methods: {},
  computed: {
    sorted() {
      return converter.FindText(this.treeDisplayData, this.query);
    }
  }
};
</script>
