import Vue from "vue";
import Vuex from "vuex";
import tables from "./tables";

Vue.use(Vuex);

const store = new Vuex.Store({
  ...tables,
});

export default store;
